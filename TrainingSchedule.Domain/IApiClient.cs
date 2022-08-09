using TrainingSchedule.Contracts;
using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.Domain
{
    public interface IApiClient
    {
        Task<ICollection<Discipline>> GetDisciplinesAsync();

        Task<ICollection<Role>> GetRolesAsync();

        Task<Lesson> CreateLessonAsync(LessonForCreationDto lessonForCreationDto);

        Task<ICollection<User>> GetUsersAsync(long? botUserId);

        Task<User> CreateUserAsync(UserForCreationDto userForCreationDto);
    }
}