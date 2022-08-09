namespace TrainingSchedule.Domain.Entities
{
    public class Lesson
    {
        public int Id { get; set; }

        public int DisciplineId { get; set; }

        public int Difficulty { get; set; }

        public DateTime Date { get; set; }

        public int TrainerId { get; set; }

        public List<int> TraineesIds { get; set; }
    }
}
