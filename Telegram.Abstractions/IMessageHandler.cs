namespace Telegram.Abstractions
{
    public interface IMessageHandler
    {
        Task<IMessageHandlingResult> HandleMessageAsync(long userId, string message);
    }
}
