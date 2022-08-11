namespace TrainingSchedule.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public long BotUserId { get; set; }

        public string Name { get; set; }

        public int RoleId { get; set; }
    }
}
