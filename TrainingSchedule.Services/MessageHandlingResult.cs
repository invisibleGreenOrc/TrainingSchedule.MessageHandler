using Telegram.Abstractions;

namespace TrainingSchedule.Services
{
    public class MessageHandlingResult : IMessageHandlingResult
    {
        public string MessageText { get; set; } = string.Empty;

        public IAllowedAnswer? AllowedAnswer { get; set; }
    }
}