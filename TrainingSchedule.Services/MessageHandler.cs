using Telegram.Abstractions;

namespace TrainingSchedule.Services
{
    public class MessageHandler : IMessageHandler
    {
        public async Task<IMessageHandlingResult> HandleMessageAsync(long userId, string message)
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

            return answer;
        }
    }
}
