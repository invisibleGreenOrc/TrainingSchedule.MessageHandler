namespace TrainingSchedule.Services.MessageService
{
    public interface IMessageHandler
    {
        Task<IMessageHandlingResult> HandleMessageAsync(long userId, string message);
    }
}
