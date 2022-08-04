using RestSharp;
using TrainingSchedule.Domain;

namespace TrainingSchedule.ApiClient
{
    public class TrainingScheduleApiClient : IApiClient
    {
        private readonly RestClient _client;

        public TrainingScheduleApiClient()
        {
            _client = new RestClient("https://localhost:7228/api/");
        }

        public async Task<ICollection<Discipline>> GetDisciplines()
        {
            var request = new RestRequest("disciplines");
            
            var response = await _client.GetAsync<ICollection<Discipline>>(request, default);
            
            return response;
        }
    }
}
