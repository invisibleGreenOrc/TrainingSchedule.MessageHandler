using TrainingSchedule.ApiClient;
using TrainingSchedule.Services;
using TrainingSchedule.Services.CommandHandlers;
using TrainingSchedule.Telegram;

namespace TrainingSchedule.ConsoleApp
{
    internal class Program
    {
        public static async Task Main()
        {
            var tgToken = Environment.GetEnvironmentVariable("tgToken", EnvironmentVariableTarget.User) ?? throw new ArgumentNullException("Не удалось получить токен.");

            var telegramClient = new TelegramClient(tgToken);
            var apiClient = new TrainingScheduleApiClient();

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

            await telegramClient.Run();
        }
    }
}