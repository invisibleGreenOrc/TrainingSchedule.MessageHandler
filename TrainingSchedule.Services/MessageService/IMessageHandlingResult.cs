namespace TrainingSchedule.Services.MessageService
{
    public interface IMessageHandlingResult
    {
        public string MessageText { get; set; }

        public IAllowedAnswer? AllowedAnswer { get; set; }
    }
}
