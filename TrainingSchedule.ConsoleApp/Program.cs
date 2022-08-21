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
        public static async Task Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<NotificationService>();
                    services.AddSingleton<IApiClient, TrainingScheduleApiClient>();
                    services.AddSingleton<IBotClient, TelegramClient>();
                })
                .Build();

            var telegramClient = ActivatorUtilities.CreateInstance<TelegramClient>(host.Services);
            var apiClient = ActivatorUtilities.CreateInstance<TrainingScheduleApiClient>(host.Services);

            var userDataService = new UsersDataService();

            var startCommandHandler = new StartCommandHandler(apiClient, telegramClient, userDataService);
            var createLessonCommandHandler = new CreateLessonCommandHandler(apiClient, telegramClient, userDataService);
            var showLessonsCommandHandler = new ShowLessonsCommandHandler(apiClient, telegramClient);
            var lessonEnrollCommandHandler = new LessonEnrollCommandHandler(apiClient, telegramClient);

            var messageService = new MessageService(telegramClient, new List<ICommandHandler> {
                    startCommandHandler,
                    createLessonCommandHandler,
                    showLessonsCommandHandler,
                    lessonEnrollCommandHandler
                });
            
            // Синхронно Асинхронно??
            host.Start();

            await telegramClient.Run();
        }
    }
}