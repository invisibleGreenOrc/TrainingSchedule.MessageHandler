using TrainingSchedule.Services;
using TrainingSchedule.Telegram;

namespace TrainingSchedule.ConsoleApp
{
    internal class Program
    {
        public static async Task Main()
        {
            var tgToken = Environment.GetEnvironmentVariable("tgToken", EnvironmentVariableTarget.User) ?? throw new ArgumentNullException("Не удалось получить токен.");

            var telegramClient = new TelegramClient(tgToken, new MessageHandler());

            await telegramClient.Run();
        }
    }
}