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
