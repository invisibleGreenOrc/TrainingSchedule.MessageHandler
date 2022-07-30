using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace TrainingSchedule.ConsoleApp
{
    internal class Program
    {
        public static async Task Main()
        {
            var tgToken = Environment.GetEnvironmentVariable("tgToken", EnvironmentVariableTarget.User) ?? throw new ArgumentNullException("Не удалось получить токен.");
            var botClient = new TelegramBotClient(tgToken);

            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                // receive all update types
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text is null)
            {
                return;
            }

            var message = update.Message;
            var messageText = message.Text;
            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            var answer = string.Empty;

            if (string.Equals(messageText, "/start"))
            {
                answer = $"Привет, {message!.From?.FirstName}!";
            }
            else
            {
                answer = "You said:\n" + messageText;
            }

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: answer,
                cancellationToken: cancellationToken);
        }

        public static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}