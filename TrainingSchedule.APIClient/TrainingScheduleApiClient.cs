using RestSharp;
using TrainingSchedule.Contracts;
using TrainingSchedule.Domain;
using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.ApiClient
{
    public class TrainingScheduleApiClient : IApiClient
    {
        private readonly RestClient _client;

        public TrainingScheduleApiClient()
        {
            _client = new RestClient("https://localhost:7228/api/");
        }

        public async Task<ICollection<Role>> GetRolesAsync()
        {
            var request = new RestRequest("roles");

            var response = await _client.GetAsync<ICollection<Role>>(request, default);

            return response;
        }

        public async Task<ICollection<Discipline>> GetDisciplinesAsync()
        {
            var request = new RestRequest("disciplines");
            
            var response = await _client.GetAsync<ICollection<Discipline>>(request, default);
            
            return response;
        }

        public async Task<ICollection<User>> GetUsersAsync(long? botUserId)
        {
            var request = new RestRequest("users");

            request.AddQueryParameter("botUserId", botUserId.Value);

            var response = await _client.GetAsync<ICollection<User>>(request, default);

            return response;
        }

        public async Task<User> CreateUserAsync(UserForCreationDto userForCreationDto)
        {
            var request = new RestRequest("users").AddJsonBody(userForCreationDto);

            var response = await _client.PostAsync<User>(request);

            return response;
        }
    }
}
