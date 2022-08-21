using System.Text;
using TrainingSchedule.Domain;
using TrainingSchedule.Domain.Entities;
using TrainingSchedule.Services.FSM;

namespace TrainingSchedule.Services.CommandHandlers
{
    public class ShowLessonsCommandHandler : ICommandHandler
    {
        public Dictionary<string, Func<IStateMachine, long, long, string, Task>> StatesAndHandlers { get; private set; }

        public Dictionary<string, string> StatesLinks { get; private set; } = new();

        private string _commandToHandle;

        private string _initialState;

        private IApiClient _apiClient;

        private IBotClient _botClient;

        public ShowLessonsCommandHandler(IApiClient apiClient, IBotClient botClient)
        {
            _commandToHandle = "/my_drills";

            AddStatesAndHandlers();

            _initialState = "ShowLessons";

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
                { "ShowLessons", ShowLessons }
            };
        }

        private async Task ShowLessons(IStateMachine stateMachine, long botUserId, long chatId, string message)
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
                await _botClient.SendMessageAsync(chatId, "Нет запланированных тренировок");
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

                await _botClient.SendMessageAsync(chatId, sb.ToString());
            }

            stateMachine.MoveToNextState();
        }
    }
}