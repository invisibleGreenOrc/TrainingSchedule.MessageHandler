using Telegram.Abstractions;

namespace TrainingSchedule.Services
{
    public class AnswerItem : IAnswerItem
    {
        public string Name { get; set; }

        public string? Value { get; set; }
    }
}