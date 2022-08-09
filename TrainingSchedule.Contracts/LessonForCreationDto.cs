namespace TrainingSchedule.Contracts
{
    public class LessonForCreationDto
    {
        public int DisciplineId { get; set; }

        public int Difficulty { get; set; }

        public DateTime Date { get; set; }

        public int TrainerId { get; set; }
    }
}
