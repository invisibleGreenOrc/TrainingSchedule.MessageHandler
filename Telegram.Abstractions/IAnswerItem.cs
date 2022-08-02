namespace Telegram.Abstractions
{
    public interface IAnswerItem
    {
        public string Name { get; set; }

        public string? Value { get; set; }
    }
}
