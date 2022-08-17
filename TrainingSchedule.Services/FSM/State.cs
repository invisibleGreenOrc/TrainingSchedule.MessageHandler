namespace TrainingSchedule.Services.FSM
{
    internal class State
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public State? NextState { get; private set;}

        public event Func<long, long, string, Task>? Entered;

        public State(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Id = id;
            Name = name;
        }

        public void SetNextState(State nextState)
        {
            NextState = nextState ?? throw new ArgumentNullException(nameof(nextState));
        }

        public async Task DoAction(long botUserId, long chatId, string message)
        {
            if (Entered is not null)
            {
                await Entered(botUserId, chatId, message);
            }
        }
    }
}
