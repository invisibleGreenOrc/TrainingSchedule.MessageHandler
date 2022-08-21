using TrainingSchedule.Services.FSM;

namespace TrainingSchedule.Services.CommandHandlers
{
    public interface ICommandHandler
    {
        Dictionary<string, Func<IStateMachine, long, long, string, Task>> StatesAndHandlers { get; }

        Dictionary<string, string> StatesLinks { get; }

        (string command, string state) GetCommandAndLinkedState();
    }
}
