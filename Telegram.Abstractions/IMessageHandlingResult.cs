namespace Telegram.Abstractions
{
    public interface IMessageHandlingResult
    {
        public string MessageText { get; set; }

        public IAllowedAnswer? AllowedAnswer { get; set; }
    }
}
