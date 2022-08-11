using TrainingSchedule.Contracts;
using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.Domain
{
    public interface IApiClient
    {
        Task<Discipline> GetDisciplineByIdAsync(int disciplineId);

        Task<ICollection<Discipline>> GetDisciplinesAsync();

        Task<ICollection<Role>> GetRolesAsync();

        Task<ICollection<Lesson>> GetFutureLessonsAsync(int? trainerId = null, int? traineeId = null);
        
        Task<Lesson> CreateLessonAsync(LessonForCreationDto lessonForCreationDto);

        Task<ICollection<User>> GetUsersAsync(long? botUserId);

        Task<User> GetUserByIdAsync(int userId);

        Task<User> CreateUserAsync(UserForCreationDto userForCreationDto);
    }
}