namespace TrainingSchedule.Contracts
{
    public class MessageToUser
    {
        public long ChatId { get; set; }

        public string Message { get; set; }

        public IAllowedAnswers AllowedAnswers { get; set; }
    }
}
