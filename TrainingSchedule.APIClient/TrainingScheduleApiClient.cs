using Microsoft.Extensions.Caching.Memory;
using RestSharp;
using TrainingSchedule.Contracts;
using TrainingSchedule.Domain;
using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.ApiClient
{
    public class TrainingScheduleApiClient : IApiClient
    {
        private readonly RestClient _client;
        private readonly IMemoryCache _memoryCache;

        public TrainingScheduleApiClient(IMemoryCache memoryCache)
        {
            _client = new RestClient("https://localhost:7228/api/");
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            _memoryCache.Set("user", "test");

            Console.WriteLine("sdfsdfsdfsdfsdfsdfsdf");

            Console.WriteLine(_memoryCache.Get<string>("user"));
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

        public async Task<ICollection<User>> GetUsersAsync(long? botUserId = null)
        {
            var request = new RestRequest("users");

            if (botUserId.HasValue)
            {
                request.AddQueryParameter("botUserId", botUserId.Value);
            }

            var response = await _client.GetAsync<ICollection<User>>(request, default);

            return response;
        }

        public async Task<User> CreateUserAsync(UserForCreationDto userForCreationDto)
        {
            var request = new RestRequest("users").AddJsonBody(userForCreationDto);

            var response = await _client.PostAsync<User>(request);

            return response;
        }

        public async Task<Lesson> CreateLessonAsync(LessonForCreationDto lessonForCreationDto)
        {
            var request = new RestRequest("lessons").AddJsonBody(lessonForCreationDto);

            var response = await _client.PostAsync<Lesson>(request);

            return response;
        }

        public async Task<Discipline> GetDisciplineByIdAsync(int disciplineId)
        {
            var request = new RestRequest("disciplines/{disciplineId}")
                .AddUrlSegment("disciplineId", disciplineId);

            var response = await _client.GetAsync<Discipline>(request, default);

            return response;
        }

        public async Task<ICollection<Lesson>> GetFutureLessonsAsync(int? trainerId = null, int? traineeId = null)
        {
            var request = new RestRequest("lessons");

            request.AddQueryParameter("dateFrom", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            if (trainerId.HasValue)
            {
                request.AddQueryParameter("trainerId", trainerId.Value);
            }

            if (traineeId.HasValue)
            {
                request.AddQueryParameter("traineeId", traineeId.Value);
            }

            var response = await _client.GetAsync<ICollection<Lesson>>(request, default);

            return response;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var request = new RestRequest("users/{userId}")
                .AddUrlSegment("userId", userId);

            var response = await _client.GetAsync<User>(request, default);

            return response;
        }

        public async Task AddLessonParticipant(int lessonId, int traineeId)
        {
            var request = new RestRequest("lessons/{lessonId}/trainees")
                .AddUrlSegment("lessonId", lessonId)
                .AddJsonBody(new { traineeId = traineeId });

            await _client.PostAsync(request);
        }
    }
}
