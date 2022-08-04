namespace TrainingSchedule.Services.MessageService
{
    public class MessageHandlingResult : IMessageHandlingResult
    {
        public string MessageText { get; set; } = string.Empty;

        public IAllowedAnswer? AllowedAnswer { get; set; }
    }
}