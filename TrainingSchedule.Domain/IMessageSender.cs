using TrainingSchedule.Contracts;

namespace TrainingSchedule.Domain
{
    public interface IMessageSender
    {
        Task SendAsync(long chatId, string messageBody, IAllowedAnswers? allowedAnswers = null);
    }
}
