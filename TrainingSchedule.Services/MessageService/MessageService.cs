using System.Data;
using System.Text;
using TrainingSchedule.Contracts;
using TrainingSchedule.Domain;
using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.Services.MessageService
{
    public class MessageService
    {
        private readonly IBotClient _botClient;
        private readonly IApiClient _apiClient;
        private Dictionary<long, string> _userStates = new();
        private Dictionary<long, string> _userNames = new();
        private Dictionary<long, (int disciplineId, int levelId, DateOnly date, TimeOnly time)> _userLesson = new();

        public MessageService(IBotClient botClient, IApiClient apiClient)
        {
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            _botClient.MessageReceived += HandleMessageAsync;

            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task HandleMessageAsync(long botUserId, long chatId, string message)
        {
            if (message == "/start")
            {
                var users = await _apiClient.GetUsersAsync(botUserId);

                var usersCount = users.Count;

                if (usersCount > 1)
                {
                    throw new Exception($"Найдено несколько пользователей с id {botUserId}. Обратитесь к администратору приложения.");
                }

                if (usersCount == 1)
                {
                    await SendMessageAsync(chatId, $"Привет, {users.First().Name}, что будем делать?");
                }
                else
                {
                    _userStates[botUserId] = "/start.ChooseName";

                    await SendMessageAsync(chatId, $"Добро пожаловать!" +
                        $"\nКак тебя зовут?");
                }
            }
            else if (message == "/create_drill")
            {
                _userStates[botUserId] = "/create_drill.ChooseDiscipline";

                var discilpines = await _apiClient.GetDisciplinesAsync();

                var answers = new AllowedAnswers
                {
                    ItemsInRow = 1,
                    Items = new List<IAnswerItem>()
                };

                foreach (var discipline in discilpines)
                {
                    var answerItem = new AnswerItem
                    {
                        Name = discipline.Name,
                        Value = discipline.Id.ToString()
                    };

                    answers.Items.Add(answerItem);
                }

                await SendMessageAsync(chatId, "Выбери дисциплину", answers);
            }
            else if (message == "/enroll_to_drill")
            {
                var lessons = await _apiClient.GetFutureLessonsAsync();

                var answers = new AllowedAnswers
                {
                    ItemsInRow = 1,
                    Items = new List<IAnswerItem>()
                };

                Discipline discipline;
                User trainer;

                foreach (var lesson in lessons.OrderBy(x => x.Date))
                {
                    discipline = await _apiClient.GetDisciplineByIdAsync(lesson.DisciplineId);
                    trainer = await _apiClient.GetUserByIdAsync(lesson.TrainerId);

                    var answerItem = new AnswerItem
                    {
                        Name = $"{lesson.Date:dd.MM.yyyy HH:mm} {discipline.Name}, сложность - {lesson.Difficulty}, тренер - {trainer.Name}",
                        Value = lesson.Id.ToString()
                    };

                    answers.Items.Add(answerItem);
                }

                await SendMessageAsync(chatId, $"Выбери тренировку", answers);

                _userStates[botUserId] = "/enroll_to_drill";
            }
            else if (message == "/my_drills")
            {
                 var users = await _apiClient.GetUsersAsync(botUserId);
                var usersCount = users.Count;

                if (usersCount > 1)
                {
                    throw new Exception($"Найдено несколько пользователей с id {botUserId}. Обратитесь к администратору приложения.");
                }

                if (usersCount == 0)
                {
                    throw new Exception($"Пользователь с id {botUserId} не найден. Попробуйте зарегистрироваться, введя команду /start.");
                }

                var userRoleId = users.First().RoleId;
                var userId = users.First().Id;

                ICollection <Lesson> lessons;

                if (userRoleId == 1)
                {
                    lessons = await _apiClient.GetFutureLessonsAsync(trainerId: userId);
                }
                else if (userRoleId == 2)
                {
                    lessons = await _apiClient.GetFutureLessonsAsync(traineeId: userId);
                }
                else
                {
                    throw new Exception($"У пользователя с botUserId {botUserId} задана недопустимая роль {userRoleId}.");
                }


                if (lessons == null || !lessons.Any())
                {
                    await SendMessageAsync(chatId, "Нет тренировок");
                }
                else
                {
                    var sb = new StringBuilder();

                    sb.AppendLine("Твои тренировки:");

                    Discipline discipline;
                    User trainer;

                    foreach (var lesson in lessons.OrderBy(x => x.Date))
                    {
                        discipline = await _apiClient.GetDisciplineByIdAsync(lesson.Id);
                        trainer = await _apiClient.GetUserByIdAsync(userId);

                        sb.AppendLine($"{lesson.Date:dd.MM.yyyy HH:mm} {discipline.Name}, сложность - {lesson.Difficulty}, тренер - {trainer.Name}");
                    }

                    await SendMessageAsync(chatId, sb.ToString());
                }
            }
            else
            {
                if (_userStates.TryGetValue(botUserId, out var state) && state == "/start.ChooseName")
                {
                    var roles = await _apiClient.GetRolesAsync();

                    if (roles.Count == 0)
                    {
                        throw new Exception($"Не найдены роли. Обратитесь к администратору приложения.");
                    }

                    _userNames[botUserId] = message.Trim();
                    _userStates[botUserId] = "/start.ChooseRole";

                    var answers = new AllowedAnswers
                    {
                        ItemsInRow = 2,
                        Items = new List<IAnswerItem>()
                    };

                    foreach (var role in roles)
                    {
                        var answerItem = new AnswerItem
                        {
                            Name = role.Name,
                            Value = role.Id.ToString()
                        };

                        answers.Items.Add(answerItem);
                    }

                    await SendMessageAsync(chatId, $"Выбери свою роль", answers);
                }
                else if (_userStates.TryGetValue(botUserId, out state) && state == "/start.ChooseRole")
                {
                    var newUser = new UserForCreationDto
                    {
                        BotUserId = botUserId,
                        Name = _userNames[botUserId],
                        RoleId = int.Parse(message)
                    };

                    await _apiClient.CreateUserAsync(newUser);
                    await SendMessageAsync(chatId, $"{newUser.Name}, профиль успешно создан! Что будем делать дальше?");

                    _userStates.Remove(botUserId);
                    _userNames.Remove(botUserId);
                }
                else if (_userStates.TryGetValue(botUserId, out state) && state == "/create_drill.ChooseDiscipline")
                {
                    _userLesson[botUserId] = (disciplineId: int.Parse(message), 0, DateOnly.MinValue, TimeOnly.MinValue);

                    _userStates[botUserId] = "/create_drill.ChooseLevel";

                    var answers = new AllowedAnswers
                    {
                        ItemsInRow = 2,
                        Items = new List<IAnswerItem>
                        {
                            new AnswerItem
                            {
                                Name = "Легкий",
                                Value = "0"
                            },
                            new AnswerItem
                            {
                                Name = "Средний",
                                Value = "1"
                            },
                            new AnswerItem
                            {
                                Name = "Сложный",
                                Value = "2"
                            }
                        }
                    };

                    await SendMessageAsync(chatId, "Выбери уровень", answers);
                }
                else if (_userStates.TryGetValue(botUserId, out state) && state == "/create_drill.ChooseLevel")
                {
                    var drillData = _userLesson[botUserId];
                    drillData.levelId = int.Parse(message);
                    _userLesson[botUserId] = drillData;

                    _userStates[botUserId] = "/create_drill.EnterDate";

                    await SendMessageAsync(chatId, "Укажи дату занятия в формате dd.mm.yyyy");
                }
                else if (_userStates.TryGetValue(botUserId, out state) && state == "/create_drill.EnterDate")
                {
                    var date = message.Trim().Split('.');

                    var drillData = _userLesson[botUserId];
                    drillData.date = new DateOnly(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                    _userLesson[botUserId] = drillData;

                    _userStates[botUserId] = "/create_drill.ChooseTime";

                    var answers = new AllowedAnswers
                    {
                        ItemsInRow = 3,
                        Items = new List<IAnswerItem>()
                    };

                    for (int i = 6; i <= 20; i++)
                    {
                        answers.Items.Add(new AnswerItem
                        {
                            Name = $"{i}:00",
                            Value = $"{i}:00"
                        });
                    }

                    await SendMessageAsync(chatId, $"Выбери время", answers);
                }
                else if (_userStates.TryGetValue(botUserId, out state) && state == "/create_drill.ChooseTime")
                {
                    var time = message.Trim().Split(':');

                    var lessonData = _userLesson[botUserId];
                    lessonData.time = new TimeOnly(int.Parse(time[0]), int.Parse(time[1]));
                    _userLesson[botUserId] = lessonData;

                    var users = await _apiClient.GetUsersAsync(botUserId);

                    var usersCount = users.Count;

                    if (usersCount > 1)
                    {
                        throw new Exception($"Найдено несколько пользователей с id {botUserId}. Обратитесь к администратору приложения.");
                    }

                    if (usersCount == 0)
                    {
                        throw new Exception($"Пользователь с id {botUserId} не найден. Попробуйте зарегистрироваться, введя команду /start.");
                    }

                    var userLessonData = _userLesson[botUserId];

                    var newLesson = new LessonForCreationDto
                    {
                        DisciplineId = userLessonData.disciplineId,
                        Difficulty = userLessonData.levelId,
                        Date = new DateTime(userLessonData.date.Year, userLessonData.date.Month, userLessonData.date.Day, userLessonData.time.Hour, userLessonData.time.Minute, 0),
                        TrainerId = users.First().Id
                    };

                    await _apiClient.CreateLessonAsync(newLesson);
                    await SendMessageAsync(chatId, $"Занятие создано {_userLesson[botUserId]}. Что будем делать дальше?");

                    _userStates.Remove(botUserId);
                    _userLesson.Remove(botUserId);
                }
            }
        }

        private async Task SendMessageAsync(long chatId, string message)
        {
            await _botClient.SendMessageAsync(chatId, message);
        }

        private async Task SendMessageAsync(long chatId, string message, IAllowedAnswers allowedAnswers)
        {
            await _botClient.SendMessageAsync(chatId, message, allowedAnswers);
        }
    }
}
