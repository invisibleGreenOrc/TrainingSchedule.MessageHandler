namespace TrainingSchedule.Domain
{
    public interface IBotClient
    {
        Task SendMessageAsync(long chatId, string message);

        event Func<long, long, string, Task> MessageReceived;
    }
}