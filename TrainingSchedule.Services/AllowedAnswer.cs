using Telegram.Abstractions;

namespace TrainingSchedule.Services
{
    public class AllowedAnswer : IAllowedAnswer
    {
        public int ItemsInRow { get; set; }

        public IEnumerable<IAnswerItem> Items { get; set; }
    }
}