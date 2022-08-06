using TrainingSchedule.Contracts;
using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.Domain
{
    public interface IApiClient
    {
        Task<ICollection<Discipline>> GetDisciplinesAsync();

        Task<ICollection<User>> GetUsersAsync(long? botUserId);

        Task<ICollection<Role>> GetRolesAsync();

        Task<User> CreateUserAsync(UserForCreationDto userForCreationDto);
    }
}