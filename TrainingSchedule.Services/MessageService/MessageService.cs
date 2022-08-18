using System.Text;
using TrainingSchedule.Contracts;
using TrainingSchedule.Domain;
using TrainingSchedule.Domain.Entities;
using TrainingSchedule.Services.FSM;

namespace TrainingSchedule.Services.MessageService
{
    public class MessageService
    {
        private readonly IBotClient _botClient;

        private readonly IApiClient _apiClient;

        private Dictionary<long, string> _userNames = new();

        private Dictionary<long, (int disciplineId, int levelId, DateOnly date, TimeOnly time)> _userLesson = new();

        private Dictionary<long, FiniteStateMachine> _userFSM = new();

        public MessageService(IBotClient botClient, IApiClient apiClient)
        {
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            _botClient.MessageReceived += HandleMessageAsync;

            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task HandleMessageAsync(long botUserId, long chatId, string message)
        {
            await GetFSMForUser(botUserId).ProcessMessage(botUserId, chatId, message);
        }

        private async Task SendMessageAsync(long chatId, string message)
        {
            await _botClient.SendMessageAsync(chatId, message);
        }

        private async Task SendMessageAsync(long chatId, string message, IAllowedAnswers allowedAnswers)
        {
            await _botClient.SendMessageAsync(chatId, message, allowedAnswers);
        }

        private async Task GreetUserOrRequestNameAsync(long botUserId, long chatId, string message)
        {
            var users = await _apiClient.GetUsersAsync(botUserId);

            var usersCount = users.Count;

            if (usersCount > 1)
            {
                throw new Exception($"Найдено несколько пользователей с id {botUserId}. Обратитесь к администратору приложения.");
            }

            if (usersCount == 1)
            {
                await SendMessageAsync(chatId, $"Привет, {users.First().Name}, выбери команду из меню.");
            }
            else
            {
                await SendMessageAsync(chatId, $"Добро пожаловать!" +
                    $"\nКак тебя зовут?");

                GetFSMForUser(botUserId).MoveToNextState();
            }
        }

        private async Task SetNameAndRequestRoleAsync(long botUserId, long chatId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                await SendMessageAsync(chatId, "Имя не может быть пустым! Введите имя еще раз.");
            }
            else
            {
                _userNames[botUserId] = message.Trim();

                var roles = await _apiClient.GetRolesAsync();

                if (roles.Count == 0)
                {
                    await SendMessageAsync(chatId, "Не найдены роли. Обратитесь к администратору приложения.");
                }

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

                GetFSMForUser(botUserId).MoveToNextState();
            }
        }

        private async Task CreateUserAsync(long botUserId, long chatId, string message)
        {
            if (int.TryParse(message, out int roleId))
            {
                var newUser = new UserForCreationDto
                {
                    BotUserId = botUserId,
                    Name = _userNames[botUserId],
                    RoleId = roleId
                };

                await _apiClient.CreateUserAsync(newUser);
                await SendMessageAsync(chatId, $"{newUser.Name}, профиль успешно создан! Что будем делать дальше?");

                _userNames.Remove(botUserId);

                GetFSMForUser(botUserId).MoveToNextState();
            }
            else
            {
                await SendMessageAsync(chatId, $"Выберите роль с помощью кнопки под сообщением выше.");
            }
        }

        private async Task RequestDisciplineAsync(long botUserId, long chatId, string message)
        {
            var discilpines = await _apiClient.GetDisciplinesAsync();

            if (discilpines.Count > 0)
            {
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
            else
            {
                await SendMessageAsync(chatId, "Дисциплины не созданы. Обратитесь к администратору приложения.");
            }

            GetFSMForUser(botUserId).MoveToNextState();
        }

        private async Task SetDisciplineAndRequestLevelAsync(long botUserId, long chatId, string message)
        {
            if (int.TryParse(message, out int disciplineId))
            {
                _userLesson[botUserId] = (disciplineId: int.Parse(message), 0, DateOnly.MinValue, TimeOnly.MinValue);

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

                GetFSMForUser(botUserId).MoveToNextState();
            }
            else
            {
                await SendMessageAsync(chatId, $"Выберите дисциплину с помощью кнопки под сообщением выше.");
            }
        }

        private async Task SetLevelAndRequestDateAsync(long botUserId, long chatId, string message)
        {
            if (int.TryParse(message, out int levelId))
            {
                var drillData = _userLesson[botUserId];
                drillData.levelId = levelId;
                _userLesson[botUserId] = drillData;

                await SendMessageAsync(chatId, "Укажи дату занятия в формате dd.mm.yyyy");

                GetFSMForUser(botUserId).MoveToNextState();
            }
            else
            {
                await SendMessageAsync(chatId, $"Выберите уровень с помощью кнопки под сообщением выше.");
            }
        }

        private async Task SetDateAndRequestTimeAsync(long botUserId, long chatId, string message)
        {
            var date = message.Trim().Split('.');

            var drillData = _userLesson[botUserId];
            drillData.date = new DateOnly(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
            _userLesson[botUserId] = drillData;

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

            GetFSMForUser(botUserId).MoveToNextState();
        }

        private async Task SetTimeAndCreateLessonAsync(long botUserId, long chatId, string message)
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

            _userLesson.Remove(botUserId);

            GetFSMForUser(botUserId).MoveToNextState();
        }

        private async Task ShowLessons(long botUserId, long chatId, string message)
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

            ICollection<Lesson> lessons;

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
                await SendMessageAsync(chatId, "Нет запланированных тренировок");
            }
            else
            {
                var sb = new StringBuilder();

                sb.AppendLine("Твои тренировки:");

                Discipline discipline;
                User trainer;

                foreach (var lesson in lessons.OrderBy(x => x.Date))
                {
                    discipline = await _apiClient.GetDisciplineByIdAsync(lesson.DisciplineId);
                    trainer = await _apiClient.GetUserByIdAsync(lesson.TrainerId);

                    sb.AppendLine($"{lesson.Date:dd.MM.yyyy HH:mm} {discipline.Name}, сложность - {lesson.Difficulty}, тренер - {trainer.Name}");
                }

                await SendMessageAsync(chatId, sb.ToString());
            }

            GetFSMForUser(botUserId).MoveToNextState();
        }

        private async Task RequestLessonToEnroll(long botUserId, long chatId, string message)
        {
            var lessons = await _apiClient.GetFutureLessonsAsync();

            if (lessons == null || !lessons.Any())
            {
                await SendMessageAsync(chatId, "Нет доступных для записи тренировок");
            }
            else
            {
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
            }

            GetFSMForUser(botUserId).MoveToNextState();
        }

        private async Task EnrollToLesson(long botUserId, long chatId, string message)
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

            await _apiClient.AddLessonParticipant(int.Parse(message), users.First().Id);

            await SendMessageAsync(chatId, $"Вы записаны.");

            GetFSMForUser(botUserId).MoveToNextState();
        }

        private FiniteStateMachine GetFSMForUser(long botUserId)
        {
            if (_userFSM.TryGetValue(botUserId, out FiniteStateMachine? fsm) && (fsm is not null))
            {
                return fsm;
            }

            fsm = new FiniteStateMachine();

            fsm.AddState("Idle");
            fsm.SetInitialState("Idle");

            fsm.AddState("Start");
            fsm.SubscribeToStateEntryEvent("Start", GreetUserOrRequestNameAsync);

            fsm.AddState("Start.NameChoice");
            fsm.SubscribeToStateEntryEvent("Start.NameChoice", SetNameAndRequestRoleAsync);

            fsm.AddState("Start.RoleChoice");
            fsm.SubscribeToStateEntryEvent("Start.RoleChoice", CreateUserAsync);

            fsm.SetNextState("Start", "Start.NameChoice");
            fsm.SetNextState("Start.NameChoice", "Start.RoleChoice");

            fsm.AddState("CreateLesson");
            fsm.SubscribeToStateEntryEvent("CreateLesson", RequestDisciplineAsync);

            fsm.AddState("CreateLesson.ChooseDiscipline");
            fsm.SubscribeToStateEntryEvent("CreateLesson.ChooseDiscipline", SetDisciplineAndRequestLevelAsync);

            fsm.AddState("CreateLesson.ChooseLevel");
            fsm.SubscribeToStateEntryEvent("CreateLesson.ChooseLevel", SetLevelAndRequestDateAsync);

            fsm.AddState("CreateLesson.EnterDate");
            fsm.SubscribeToStateEntryEvent("CreateLesson.EnterDate", SetDateAndRequestTimeAsync);

            fsm.AddState("CreateLesson.ChooseTime");
            fsm.SubscribeToStateEntryEvent("CreateLesson.ChooseTime", SetTimeAndCreateLessonAsync);

            fsm.SetNextState("CreateLesson", "CreateLesson.ChooseDiscipline");
            fsm.SetNextState("CreateLesson.ChooseDiscipline", "CreateLesson.ChooseLevel");
            fsm.SetNextState("CreateLesson.ChooseLevel", "CreateLesson.EnterDate");
            fsm.SetNextState("CreateLesson.EnterDate", "CreateLesson.ChooseTime");

            fsm.AddState("ShowLessons");
            fsm.SubscribeToStateEntryEvent("ShowLessons", ShowLessons);

            fsm.AddState("EnrollToLesson.ShowLessons");
            fsm.SubscribeToStateEntryEvent("EnrollToLesson.ShowLessons", RequestLessonToEnroll);

            fsm.AddState("EnrollToLesson.Enroll");
            fsm.SubscribeToStateEntryEvent("EnrollToLesson.Enroll", EnrollToLesson);

            fsm.SetNextState("EnrollToLesson.ShowLessons", "EnrollToLesson.Enroll");

            fsm.AddCommandToGoToState("/start", "Start");
            fsm.AddCommandToGoToState("/create_drill", "CreateLesson");
            fsm.AddCommandToGoToState("/my_drills", "ShowLessons");
            fsm.AddCommandToGoToState("/enroll_to_drill", "EnrollToLesson.ShowLessons");

            _userFSM.Add(botUserId, fsm);
            return fsm;
        }
    }
}
