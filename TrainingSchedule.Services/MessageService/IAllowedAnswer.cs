namespace TrainingSchedule.Services.MessageService
{
    public interface IAllowedAnswer
    {
        public int ItemsInRow { get; set; }

        public IEnumerable<IAnswerItem> Items { get; set; }
    }
}