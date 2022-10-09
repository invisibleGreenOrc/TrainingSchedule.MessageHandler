namespace TrainingSchedule.Contracts
{
    public class MessageFromUser
    {
        public long BotUserId { get; set; }

        public long ChatId { get; set; }

        public string Body { get; set; }
    }
}
