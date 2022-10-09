using TrainingSchedule.Contracts;

namespace TrainingSchedule.Domain
{
    public interface IMessageReceiver
    {
        event Func<MessageFromUser, Task>? MessageReceived;
        void Start();
    }
}