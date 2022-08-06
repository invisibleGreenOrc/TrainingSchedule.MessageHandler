using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.Services.MessageService
{
    public interface IMessageHandlingResult
    {
        public string MessageText { get; set; }

        public IAllowedAnswers? AllowedAnswer { get; set; }
    }
}
