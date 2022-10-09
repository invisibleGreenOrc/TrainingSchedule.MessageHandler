using Microsoft.Extensions.Hosting;
using System.Text;
using TrainingSchedule.Domain;
using TrainingSchedule.Domain.Entities;

namespace TrainingSchedule.Services.BackgroundServices
{
    public class NotificationService : BackgroundService
    {
        private IApiClient _apiClient;

        private IMessageSender _messageSender;

        public NotificationService(IApiClient apiClient, IMessageSender messageSender)
        {
            _apiClient = apiClient;
            _messageSender = messageSender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
             while (!stoppingToken.IsCancellationRequested)
            {
                var users = await _apiClient.GetUsersAsync();

                foreach (var user in users)
                {
                    var userRoleId = users.First().RoleId;
                    var userId = users.First().Id;

                    ICollection<Lesson> lessons = new List<Lesson>();

                    if (userRoleId == 1)
                    {
                        lessons = await _apiClient.GetFutureLessonsAsync(trainerId: userId);
                    }
                    else if (userRoleId == 2)
                    {
                        lessons = await _apiClient.GetFutureLessonsAsync(traineeId: userId);
                    }

                    if (lessons != null & lessons.Any())
                    {
                        var sb = new StringBuilder();

                        sb.AppendLine("Твои тренировки:");

                        Discipline discipline;
                        User trainer;

                        foreach (var lesson in lessons.OrderBy(x => x.Date))
                        {
                            discipline = await _apiClient.GetDisciplineByIdAsync(lesson.DisciplineId);
                            trainer = await _apiClient.GetUserByIdAsync(lesson.TrainerId);

                            sb.AppendLine($"{lesson.Date:dd.MM.yyyy HH:mm} {discipline.Name}, сложность - {lesson.Difficulty}, тренер - {trainer.Name}");
                        }

                        await _messageSender.SendAsync(user.BotUserId, sb.ToString());
                    }
                }

                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}
