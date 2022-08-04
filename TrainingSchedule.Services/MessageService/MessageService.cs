using TrainingSchedule.Domain;

namespace TrainingSchedule.Services.MessageService
{
    public class MessageService
    {
        private readonly IBotClient _botClient;
        private readonly IApiClient _apiClient;

        public MessageService(IBotClient botClient, IApiClient apiClient)
        {
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            _botClient.MessageReceived += HandleMessage;

            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task HandleMessage(long userId, long chatId, string message)
        {
            string? answerText;

            if (string.Equals(message, "/start"))
            {
                answerText = $"Привет!";
            }
            else
            {
                answerText = "You said:\n" + message;
            }

            var answer = new MessageHandlingResult
            {
                MessageText = answerText,
                AllowedAnswer = null
            };

            await SendMessageAsync(chatId, answerText + "!!!!!!");

            var disciplines = await _apiClient.GetDisciplines();

            string response = string.Empty;

            foreach (var item in disciplines)
            {
                response = response + item.Name;
            }

            await SendMessageAsync(chatId, response);
        }

        private async Task SendMessageAsync(long chatId, string message)
        {
            await _botClient.SendMessageAsync(chatId, message);
        }
    }
}
