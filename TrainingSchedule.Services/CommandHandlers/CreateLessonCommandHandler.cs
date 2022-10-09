using TrainingSchedule.Contracts;
using TrainingSchedule.Domain;
using TrainingSchedule.Domain.Entities;
using TrainingSchedule.Services.FSM;

namespace TrainingSchedule.Services.CommandHandlers
{
    public class CreateLessonCommandHandler : ICommandHandler
    {
        public Dictionary<string, Func<IStateMachine, long, long, string, Task>> StatesAndHandlers { get; private set; }

        public Dictionary<string, string> StatesLinks { get; private set; }

        private string _commandToHandle;

        private string _initialState;

        private IApiClient _apiClient;

        private IMessageSender _messageSender;

        private IUsersDataService _usersDataService;

        public CreateLessonCommandHandler(IApiClient apiClient, IMessageSender messageSender, IUsersDataService usersDataService)
        {
            _commandToHandle = "/create_drill";

            AddStatesAndHandlers();
            AddStatesLinks();

            _initialState = "CreateLesson";

            _apiClient = apiClient;
            _messageSender = messageSender;
            _usersDataService = usersDataService;
        }

        public (string command, string state) GetCommandAndLinkedState()
        {
            return (_commandToHandle, _initialState);
        }

        private void AddStatesAndHandlers()
        {
            StatesAndHandlers = new Dictionary<string, Func<IStateMachine, long, long, string, Task>>
            {
                { "CreateLesson", RequestDisciplineAsync },
                { "CreateLesson.ChooseDiscipline", SetDisciplineAndRequestLevelAsync },
                { "CreateLesson.ChooseLevel", SetLevelAndRequestDateAsync },
                { "CreateLesson.EnterDate", SetDateAndRequestTimeAsync },
                { "CreateLesson.ChooseTime", SetTimeAndCreateLessonAsync }
            };
        }

        private void AddStatesLinks()
        {
            StatesLinks = new Dictionary<string, string>
            {
                { "CreateLesson", "CreateLesson.ChooseDiscipline" },
                { "CreateLesson.ChooseDiscipline", "CreateLesson.ChooseLevel" },
                { "CreateLesson.ChooseLevel", "CreateLesson.EnterDate" },
                { "CreateLesson.EnterDate", "CreateLesson.ChooseTime" }
            };
        }

        private async Task RequestDisciplineAsync(IStateMachine stateMachine, long botUserId, long chatId, string message)
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

                await _messageSender.SendAsync(chatId, "Выбери дисциплину", answers);
            }
            else
            {
                await _messageSender.SendAsync(chatId, "Дисциплины не созданы. Обратитесь к администратору приложения.");
            }

            stateMachine.MoveToNextState();
        }

        private async Task SetDisciplineAndRequestLevelAsync(IStateMachine stateMachine, long botUserId, long chatId, string message)
        {
            if (int.TryParse(message, out int disciplineId))
            {
                _usersDataService.AddUserLesson(botUserId, (disciplineId, 0, DateOnly.MinValue, TimeOnly.MinValue));

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

                await _messageSender.SendAsync(chatId, "Выбери уровень", answers);

                stateMachine.MoveToNextState();
            }
            else
            {
                await _messageSender.SendAsync(chatId, $"Выберите дисциплину с помощью кнопки под сообщением выше.");
            }
        }

        private async Task SetLevelAndRequestDateAsync(IStateMachine stateMachine, long botUserId, long chatId, string message)
        {
            if (int.TryParse(message, out int levelId))
            {
                var lessonData = _usersDataService.GetUserLesson(botUserId);
                lessonData.levelId = levelId;
                _usersDataService.AddUserLesson(botUserId, lessonData);

                await _messageSender.SendAsync(chatId, "Укажи дату занятия в формате dd.mm.yyyy");

                stateMachine.MoveToNextState();
            }
            else
            {
                await _messageSender.SendAsync(chatId, $"Выберите уровень с помощью кнопки под сообщением выше.");
            }
        }

        private async Task SetDateAndRequestTimeAsync(IStateMachine stateMachine, long botUserId, long chatId, string message)
        {
            var date = message.Trim().Split('.');

            var lessonData = _usersDataService.GetUserLesson(botUserId);
            lessonData.date = new DateOnly(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
            _usersDataService.AddUserLesson(botUserId, lessonData);

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

            await _messageSender.SendAsync(chatId, $"Выбери время", answers);

            stateMachine.MoveToNextState();
        }

        private async Task SetTimeAndCreateLessonAsync(IStateMachine stateMachine, long botUserId, long chatId, string message)
        {
            var time = message.Trim().Split(':');

            var lessonData = _usersDataService.GetUserLesson(botUserId);
            lessonData.time = new TimeOnly(int.Parse(time[0]), int.Parse(time[1]));
            _usersDataService.AddUserLesson(botUserId, lessonData);

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

            var userLessonData = _usersDataService.GetUserLesson(botUserId);

            var newLesson = new LessonForCreationDto
            {
                DisciplineId = userLessonData.disciplineId,
                Difficulty = userLessonData.levelId,
                Date = new DateTime(userLessonData.date.Year, userLessonData.date.Month, userLessonData.date.Day, userLessonData.time.Hour, userLessonData.time.Minute, 0),
                TrainerId = users.First().Id
            };

            await _apiClient.CreateLessonAsync(newLesson);
            await _messageSender.SendAsync(chatId, $"Занятие создано {_usersDataService.GetUserLesson(botUserId)}. Что будем делать дальше?");

            _usersDataService.RemoveUserLesson(botUserId);

            stateMachine.MoveToNextState();
        }
    }
}