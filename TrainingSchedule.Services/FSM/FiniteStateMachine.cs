namespace TrainingSchedule.Services.FSM
{
    public class FiniteStateMachine
    {
        private List<State> _states;

        private State? _initialState;

        private State? _currentState;

        private Dictionary<string, State> _commandsToSetState;

        public FiniteStateMachine()
        {
            _states = new List<State>();
            _commandsToSetState = new Dictionary<string, State>();
        }

        public void AddState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new ArgumentNullException(nameof(stateName));
            }

            int id;

            if (_states.Count > 0)
            {
                id = _states.Last().Id + 1;
            }
            else
            {
                id = 0;
            }

            _states.Add(new State(id, stateName));
        }

        public void SetInitialState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new ArgumentNullException(nameof(stateName));
            }

            _initialState = GetStateByName(stateName);
            _currentState = _initialState;
        }

        public void SetNextState(string stateName, string nextStateName)
        {
            var state = GetStateByName(stateName);
            var nextState = GetStateByName(nextStateName);
            state.SetNextState(nextState);
        }

        public void AddCommandToGoToState(string command, string stateName)
        {
            var state = GetStateByName(stateName);
            _commandsToSetState.Add(command, state);
        }

        public void SubscribeToStateEntryEvent(string stateName, Func<long, long, string, Task> action)
        {
            GetStateByName(stateName).Entered += action;
        }

        public async Task ProcessMessage(long botUserId, long chatId, string message)
        {
            if (_currentState is null)
            {
                throw new InvalidOperationException("Не задано текущее состояние.");
            }

            if (_commandsToSetState.ContainsKey(message))
            {
                _currentState = _commandsToSetState[message];
            }

            await _currentState.DoAction(botUserId, chatId, message);
        }

        public void MoveToNextState()
        {
            if (_currentState is null)
            {
                throw new InvalidOperationException("Не задано текущее состояние.");
            }

            _currentState = _currentState.NextState ?? _initialState;
        }

        private State GetStateByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _states.Find(x => string.Equals(x.Name, name)) ?? throw new InvalidOperationException($"Не найдено состояние с именем {name}.");
        }
    }
}
