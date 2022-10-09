using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrainingSchedule.ApiClient;
using TrainingSchedule.Services;
using TrainingSchedule.Services.CommandHandlers;
using TrainingSchedule.Services.BackgroundServices;
using TrainingSchedule.Telegram;
using TrainingSchedule.Domain;

namespace TrainingSchedule.ConsoleApp
{
    internal class Program
    {
        public static void Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<MessageProcessingService>();
                    services.AddHostedService<NotificationService>();
                    services.AddSingleton<IApiClient, TrainingScheduleApiClient>();
                    services.AddSingleton<IBotClient, TelegramClient>();
                    services.AddSingleton<ICommandHandler, StartCommandHandler>();
                    services.AddSingleton<ICommandHandler, CreateLessonCommandHandler>();
                    services.AddSingleton<ICommandHandler, ShowLessonsCommandHandler>();
                    services.AddSingleton<ICommandHandler, LessonEnrollCommandHandler>();
                    services.AddSingleton<IUsersDataService, UsersDataService>();
                    services.AddMemoryCache();
                })
                .Build();

            host.Start();

            Console.ReadKey();
        }
    }
}