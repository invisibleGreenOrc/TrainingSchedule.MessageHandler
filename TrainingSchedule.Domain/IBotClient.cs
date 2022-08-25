using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.Domain
{
    public interface IBotClient
    {
        Task StartAsync();
        
        Task SendMessageAsync(long chatId, string message);

        Task SendMessageAsync(long chatId, string message, IAllowedAnswers allowedAnswers);

        event Func<long, long, string, Task> MessageReceived;
    }
}