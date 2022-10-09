namespace TrainingSchedule.Contracts
{
    public interface IAllowedAnswers
    {
        public int ItemsInRow { get; set; }

        public ICollection<IAnswerItem> Items { get; set; }
    }
}