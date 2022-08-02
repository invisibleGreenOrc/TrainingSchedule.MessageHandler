using Telegram.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TrainingSchedule.Telegram
{
    public class TelegramClient
    {
        private readonly string _telegramToken;
        private readonly IMessageHandler _messageHandler;

        public TelegramClient(string telegramToken, IMessageHandler messageHandler)
        {
            _telegramToken = telegramToken ?? throw new ArgumentNullException("Не задан токен.");
            _messageHandler = messageHandler ?? throw new ArgumentNullException("Не задан messageHandler.");
        }

        public async Task Run()
        {
            var botClient = new TelegramBotClient(_telegramToken);

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

            var answer = await _messageHandler.HandleMessageAsync(userId, messageText);

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: answer.MessageText,
                cancellationToken: cancellationToken);
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
    }
}