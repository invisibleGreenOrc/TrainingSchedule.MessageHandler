namespace TrainingSchedule.Domain.Entities
{
    public class AllowedAnswers : IAllowedAnswers
    {
        public int ItemsInRow { get; set; }

        public ICollection<IAnswerItem> Items { get; set; }
    }
}