namespace TrainingSchedule.Services.FSM
{
    public class State
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public State? NextState { get; private set; }

        public event Func<FiniteStateMachine, long, long, string, Task>? Entered;

        private FiniteStateMachine _stateMachine;

        public State(int id, string name, FiniteStateMachine stateMachine)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Id = id;
            Name = name;
            _stateMachine = stateMachine;
        }

        public void SetNextState(State nextState)
        {
            NextState = nextState ?? throw new ArgumentNullException(nameof(nextState));
        }

        public async Task DoAction(long botUserId, long chatId, string message)
        {
            if (Entered is not null)
            {
                await Entered(_stateMachine, botUserId, chatId, message);
            }
        }
    }
}
