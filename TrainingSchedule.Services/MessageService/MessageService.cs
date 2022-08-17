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

        private Dictionary<long, string> _userStates = new();

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

            //if (message == "/start")
            //{
            //    await GetFSMForUser(botUserId).ProcessMessage(botUserId, chatId, message);
            //    //await GreetUserOrRequestNameAsync(botUserId, chatId, message);
            //}
            //else if (message == "/create_drill")
            //{
            //    _userStates[botUserId] = "/create_drill.ChooseDiscipline";

            //    var discilpines = await _apiClient.GetDisciplinesAsync();

            //    var answers = new AllowedAnswers
            //    {
            //        ItemsInRow = 1,
            //        Items = new List<IAnswerItem>()
            //    };

            //    foreach (var discipline in discilpines)
            //    {
            //        var answerItem = new AnswerItem
            //        {
            //            Name = discipline.Name,
            //            Value = discipline.Id.ToString()
            //        };

            //        answers.Items.Add(answerItem);
            //    }

            //    await SendMessageAsync(chatId, "Выбери дисциплину", answers);
            //}
            //else if (message == "/enroll_to_drill")
            //{
            //    var lessons = await _apiClient.GetFutureLessonsAsync();

            //    if (lessons == null || !lessons.Any())
            //    {
            //        await SendMessageAsync(chatId, "Нет доступных для записи тренировок");
            //    }
            //    else
            //    {
            //        var answers = new AllowedAnswers
            //        {
            //            ItemsInRow = 1,
            //            Items = new List<IAnswerItem>()
            //        };

            //        Discipline discipline;
            //        User trainer;

            //        foreach (var lesson in lessons.OrderBy(x => x.Date))
            //        {
            //            discipline = await _apiClient.GetDisciplineByIdAsync(lesson.DisciplineId);
            //            trainer = await _apiClient.GetUserByIdAsync(lesson.TrainerId);

            //            var answerItem = new AnswerItem
            //            {
            //                Name = $"{lesson.Date:dd.MM.yyyy HH:mm} {discipline.Name}, сложность - {lesson.Difficulty}, тренер - {trainer.Name}",
            //                Value = lesson.Id.ToString()
            //            };

            //            answers.Items.Add(answerItem);
            //        }

            //        await SendMessageAsync(chatId, $"Выбери тренировку", answers);
            //    }

            //    _userStates[botUserId] = "/enroll_to_drill";
            //}
            //else if (message == "/my_drills")
            //{
            //    var users = await _apiClient.GetUsersAsync(botUserId);
            //    var usersCount = users.Count;

            //    if (usersCount > 1)
            //    {
            //        throw new Exception($"Найдено несколько пользователей с id {botUserId}. Обратитесь к администратору приложения.");
            //    }

            //    if (usersCount == 0)
            //    {
            //        throw new Exception($"Пользователь с id {botUserId} не найден. Попробуйте зарегистрироваться, введя команду /start.");
            //    }

            //    var userRoleId = users.First().RoleId;
            //    var userId = users.First().Id;

            //    ICollection<Lesson> lessons;

            //    if (userRoleId == 1)
            //    {
            //        lessons = await _apiClient.GetFutureLessonsAsync(trainerId: userId);
            //    }
            //    else if (userRoleId == 2)
            //    {
            //        lessons = await _apiClient.GetFutureLessonsAsync(traineeId: userId);
            //    }
            //    else
            //    {
            //        throw new Exception($"У пользователя с botUserId {botUserId} задана недопустимая роль {userRoleId}.");
            //    }


            //    if (lessons == null || !lessons.Any())
            //    {
            //        await SendMessageAsync(chatId, "Нет запланированных тренировок");
            //    }
            //    else
            //    {
            //        var sb = new StringBuilder();

            //        sb.AppendLine("Твои тренировки:");

            //        Discipline discipline;
            //        User trainer;

            //        foreach (var lesson in lessons.OrderBy(x => x.Date))
            //        {
            //            discipline = await _apiClient.GetDisciplineByIdAsync(lesson.DisciplineId);
            //            trainer = await _apiClient.GetUserByIdAsync(lesson.TrainerId);

            //            sb.AppendLine($"{lesson.Date:dd.MM.yyyy HH:mm} {discipline.Name}, сложность - {lesson.Difficulty}, тренер - {trainer.Name}");
            //        }

            //        await SendMessageAsync(chatId, sb.ToString());
            //    }
            //}
            //else
            //{
            //    if (_userStates.TryGetValue(botUserId, out var state) && state == "/start.ChooseName")
            //    {
            //        await GetFSMForUser(botUserId).ProcessMessage(botUserId, chatId, message);
            //        //await SetNameAndRequestRoleAsync(botUserId, chatId, message);
            //    }
            //    else if (_userStates.TryGetValue(botUserId, out state) && state == "/start.ChooseRole")
            //    {
            //        await GetFSMForUser(botUserId).ProcessMessage(botUserId, chatId, message);
            //        //await CreateUserAsync(botUserId, chatId, message);
            //    }
            //    else if (_userStates.TryGetValue(botUserId, out state) && state == "/create_drill.ChooseDiscipline")
            //    {
            //        _userLesson[botUserId] = (disciplineId: int.Parse(message), 0, DateOnly.MinValue, TimeOnly.MinValue);

            //        _userStates[botUserId] = "/create_drill.ChooseLevel";

            //        var answers = new AllowedAnswers
            //        {
            //            ItemsInRow = 2,
            //            Items = new List<IAnswerItem>
            //            {
            //                new AnswerItem
            //                {
            //                    Name = "Легкий",
            //                    Value = "0"
            //                },
            //                new AnswerItem
            //                {
            //                    Name = "Средний",
            //                    Value = "1"
            //                },
            //                new AnswerItem
            //                {
            //                    Name = "Сложный",
            //                    Value = "2"
            //                }
            //            }
            //        };

            //        await SendMessageAsync(chatId, "Выбери уровень", answers);
            //    }
            //    else if (_userStates.TryGetValue(botUserId, out state) && state == "/create_drill.ChooseLevel")
            //    {
            //        var drillData = _userLesson[botUserId];
            //        drillData.levelId = int.Parse(message);
            //        _userLesson[botUserId] = drillData;

            //        _userStates[botUserId] = "/create_drill.EnterDate";

            //        await SendMessageAsync(chatId, "Укажи дату занятия в формате dd.mm.yyyy");
            //    }
            //    else if (_userStates.TryGetValue(botUserId, out state) && state == "/create_drill.EnterDate")
            //    {
            //        var date = message.Trim().Split('.');

            //        var drillData = _userLesson[botUserId];
            //        drillData.date = new DateOnly(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
            //        _userLesson[botUserId] = drillData;

            //        _userStates[botUserId] = "/create_drill.ChooseTime";

            //        var answers = new AllowedAnswers
            //        {
            //            ItemsInRow = 3,
            //            Items = new List<IAnswerItem>()
            //        };

            //        for (int i = 6; i <= 20; i++)
            //        {
            //            answers.Items.Add(new AnswerItem
            //            {
            //                Name = $"{i}:00",
            //                Value = $"{i}:00"
            //            });
            //        }

            //        await SendMessageAsync(chatId, $"Выбери время", answers);
            //    }
            //    else if (_userStates.TryGetValue(botUserId, out state) && state == "/create_drill.ChooseTime")
            //    {
            //        var time = message.Trim().Split(':');

            //        var lessonData = _userLesson[botUserId];
            //        lessonData.time = new TimeOnly(int.Parse(time[0]), int.Parse(time[1]));
            //        _userLesson[botUserId] = lessonData;

            //        var users = await _apiClient.GetUsersAsync(botUserId);

            //        var usersCount = users.Count;

            //        if (usersCount > 1)
            //        {
            //            throw new Exception($"Найдено несколько пользователей с id {botUserId}. Обратитесь к администратору приложения.");
            //        }

            //        if (usersCount == 0)
            //        {
            //            throw new Exception($"Пользователь с id {botUserId} не найден. Попробуйте зарегистрироваться, введя команду /start.");
            //        }

            //        var userLessonData = _userLesson[botUserId];

            //        var newLesson = new LessonForCreationDto
            //        {
            //            DisciplineId = userLessonData.disciplineId,
            //            Difficulty = userLessonData.levelId,
            //            Date = new DateTime(userLessonData.date.Year, userLessonData.date.Month, userLessonData.date.Day, userLessonData.time.Hour, userLessonData.time.Minute, 0),
            //            TrainerId = users.First().Id
            //        };

            //        await _apiClient.CreateLessonAsync(newLesson);
            //        await SendMessageAsync(chatId, $"Занятие создано {_userLesson[botUserId]}. Что будем делать дальше?");

            //        _userStates.Remove(botUserId);
            //        _userLesson.Remove(botUserId);
            //    }
            //    else if (_userStates.TryGetValue(botUserId, out state) && state == "/enroll_to_drill")
            //    {
            //        var users = await _apiClient.GetUsersAsync(botUserId);

            //        var usersCount = users.Count;

            //        if (usersCount > 1)
            //        {
            //            throw new Exception($"Найдено несколько пользователей с id {botUserId}. Обратитесь к администратору приложения.");
            //        }

            //        if (usersCount == 0)
            //        {
            //            throw new Exception($"Пользователь с id {botUserId} не найден. Попробуйте зарегистрироваться, введя команду /start.");
            //        }

            //        await _apiClient.AddLessonParticipant(int.Parse(message), users.First().Id);

            //        await SendMessageAsync(chatId, $"Вы записаны.");
            //        _userStates.Remove(botUserId);
            //    }
            //}


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
                await SendMessageAsync(chatId, $"Привет, {users.First().Name}, что будем делать?");
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

                _userStates.Remove(botUserId);
                _userNames.Remove(botUserId);

                GetFSMForUser(botUserId).MoveToNextState();
            }
            else
            {
                await SendMessageAsync(chatId, $"Выберите роль с помощью кнопки под сообщением выше.");
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

            fsm.AddCommandToGoToState("/start", "Start");

            _userFSM.Add(botUserId, fsm);
            return fsm;
        }
    }
}
