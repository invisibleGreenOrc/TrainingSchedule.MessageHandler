using TrainingSchedule.Contracts;
using TrainingSchedule.Domain.Entities;
using TrainingSchedule.Domain;
using TrainingSchedule.Services.FSM;

namespace TrainingSchedule.Services.CommandHandlers
{
    public class StartCommandHandler : ICommandHandler
    {
        public Dictionary<string, Func<IStateMachine, long, long, string, Task>> StatesAndHandlers { get; private set; }

        public Dictionary<string, string> StatesLinks { get; private set; }

        private string _commandToHandle;

        private string _initialState;

        private IApiClient _apiClient;

        private IBotClient _botClient;

        private IUsersDataService _usersDataService; 

        public StartCommandHandler(IApiClient apiClient, IBotClient botClient, IUsersDataService usersDataService)
        {
            _commandToHandle = "/start";

            AddStatesAndHandlers();
            AddStatesLinks();

            _initialState = "Start";

            _apiClient = apiClient;
            _botClient = botClient;
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
                { "Start", GreetUserOrRequestNameAsync },
                { "Start.NameChoice", SetNameAndRequestRoleAsync },
                { "Start.RoleChoice", CreateUserAsync }
            };
        }

        private void AddStatesLinks()
        {
            StatesLinks = new Dictionary<string, string>
            {
                { "Start", "Start.NameChoice" },
                { "Start.NameChoice", "Start.RoleChoice" }
            };
        }

        private async Task GreetUserOrRequestNameAsync(IStateMachine stateMachine, long botUserId, long chatId, string message)
        {
            var users = await _apiClient.GetUsersAsync(botUserId);

            var usersCount = users.Count;

            if (usersCount > 1)
            {
                throw new Exception($"Найдено несколько пользователей с id {botUserId}. Обратитесь к администратору приложения.");
            }

            if (usersCount == 1)
            {
                await _botClient.SendMessageAsync(chatId, $"Привет, {users.First().Name}, выбери команду из меню.");
            }
            else
            {
                await _botClient.SendMessageAsync(chatId, $"Добро пожаловать!" +
                    $"\nКак тебя зовут?");

                stateMachine.MoveToNextState();
            }
        }

        private async Task SetNameAndRequestRoleAsync(IStateMachine stateMachine, long botUserId, long chatId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                await _botClient.SendMessageAsync(chatId, "Имя не может быть пустым! Введите имя еще раз.");
            }
            else
            {
                _usersDataService.AddUserName(botUserId, message.Trim());

                var roles = await _apiClient.GetRolesAsync();

                if (roles.Count == 0)
                {
                    await _botClient.SendMessageAsync(chatId, "Не найдены роли. Обратитесь к администратору приложения.");
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

                await _botClient.SendMessageAsync(chatId, $"Выбери свою роль", answers);

                stateMachine.MoveToNextState();
            }
        }

        private async Task CreateUserAsync(IStateMachine stateMachine, long botUserId, long chatId, string message)
        {
            if (int.TryParse(message, out int roleId))
            {
                var newUser = new UserForCreationDto
                {
                    BotUserId = botUserId,
                    Name = _usersDataService.GetUserName(botUserId),
                    RoleId = roleId
                };

                await _apiClient.CreateUserAsync(newUser);
                await _botClient.SendMessageAsync(chatId, $"{newUser.Name}, профиль успешно создан! Что будем делать дальше?");

                _usersDataService.RemoveUserName(botUserId);

                stateMachine.MoveToNextState();
            }
            else
            {
                await _botClient.SendMessageAsync(chatId, $"Выберите роль с помощью кнопки под сообщением выше.");
            }
        }
    }
}