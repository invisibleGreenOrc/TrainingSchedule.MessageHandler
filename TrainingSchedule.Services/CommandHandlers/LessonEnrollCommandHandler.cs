using TrainingSchedule.Domain;
using TrainingSchedule.Domain.Entities;
using TrainingSchedule.Services.FSM;

namespace TrainingSchedule.Services.CommandHandlers
{
    public class LessonEnrollCommandHandler : ICommandHandler
    {
        public Dictionary<string, Func<IStateMachine, long, long, string, Task>> StatesAndHandlers { get; private set; }

        public Dictionary<string, string> StatesLinks { get; private set; }

        private string _commandToHandle;

        private string _initialState;

        private IApiClient _apiClient;

        private IBotClient _botClient;

        public LessonEnrollCommandHandler(IApiClient apiClient, IBotClient botClient)
        {
            _commandToHandle = "/enroll_to_drill";

            AddStatesAndHandlers();
            AddStatesLinks();

            _initialState = "EnrollToLesson.ShowLessons";

            _apiClient = apiClient;
            _botClient = botClient;
        }

        public (string command, string state) GetCommandAndLinkedState()
        {
            return (_commandToHandle, _initialState);
        }

        private void AddStatesAndHandlers()
        {
            StatesAndHandlers = new Dictionary<string, Func<IStateMachine, long, long, string, Task>>
            {
                { "EnrollToLesson.ShowLessons", RequestLessonToEnroll },
                { "EnrollToLesson.Enroll", EnrollToLesson }
            };
        }

        private void AddStatesLinks()
        {
            StatesLinks = new Dictionary<string, string>
            {
                { "EnrollToLesson.ShowLessons", "EnrollToLesson.Enroll" }
            };
        }

        private async Task RequestLessonToEnroll(IStateMachine stateMachine, long botUserId, long chatId, string message)
        {
            var lessons = await _apiClient.GetFutureLessonsAsync();

            if (lessons == null || !lessons.Any())
            {
                await _botClient.SendMessageAsync(chatId, "Нет доступных для записи тренировок");
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

                await _botClient.SendMessageAsync(chatId, $"Выбери тренировку", answers);
            }

            stateMachine.MoveToNextState();
        }

        private async Task EnrollToLesson(IStateMachine stateMachine, long botUserId, long chatId, string message)
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

            await _botClient.SendMessageAsync(chatId, $"Вы записаны.");

            stateMachine.MoveToNextState();
        }
    }
}