namespace TrainingSchedule.Domain
{
    public interface IApiClient
    {
        Task<ICollection<Discipline>> GetDisciplines();
    }
}