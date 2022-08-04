using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TrainingSchedule.Domain;

namespace TrainingSchedule.Telegram
{
    public class TelegramClient : IBotClient
    {
        private readonly TelegramBotClient _botClient;

        public event Func<long, long, string, Task> MessageReceived;

        public TelegramClient(string telegramToken)
        {
            if (telegramToken is null)
            {
                throw new ArgumentNullException("Не задан токен.");
            }

            _botClient = new TelegramBotClient(telegramToken);
        }

        public async Task Run()
        {
            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                // receive all update types
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            _botClient.StartReceiving(
                        updateHandler: HandleUpdateAsync,
                        pollingErrorHandler: HandlePollingErrorAsync,
                        receiverOptions: receiverOptions,
                        cancellationToken: cts.Token
                    );

            var me = await _botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text is null)
            {
                return;
            }

            var message = update.Message;
            var messageText = message.Text;
            var chatId = message.Chat.Id;
            var userId = message.From.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            MessageReceived?.Invoke(userId, chatId, messageText);
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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

        public async Task SendMessageAsync(long chatId, string message)
        {
            Message sentMessage = await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                cancellationToken: default);
        }
    }
}