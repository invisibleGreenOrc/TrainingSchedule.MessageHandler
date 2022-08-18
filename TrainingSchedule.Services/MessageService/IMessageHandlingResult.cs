using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.Services.MessageService
{
    public interface IMessageHandlingResult
    {
        string MessageText { get; set; }

        IAllowedAnswers? AllowedAnswer { get; set; }
    }
}
