using TrainingSchedule.Services.CommandHandlers;

namespace TrainingSchedule.Services.FSM
{
    public class FiniteStateMachineBuilder
    {
        private IEnumerable<ICommandHandler> _commandHandlers = new List<ICommandHandler>();

        public FiniteStateMachineBuilder(IEnumerable<ICommandHandler> commandHandlers)
        {
            _commandHandlers = commandHandlers;
        }

        public FiniteStateMachine Build()
        {
            var stateMachine = new FiniteStateMachine();

            stateMachine.AddState("Idle");
            stateMachine.SetInitialState("Idle");

            foreach (var handler in _commandHandlers)
            {
                foreach (var item in handler.StatesAndHandlers)
                {
                    stateMachine.AddState(item.Key);
                    stateMachine.SubscribeToStateEntryEvent(item.Key, item.Value);
                }

                foreach (var item in handler.StatesLinks)
                {
                    stateMachine.SetNextState(item.Key, item.Value);
                }

                var (command, state) = handler.GetCommandAndLinkedState();

                stateMachine.AddCommandToGoToState(command, state);
            }

            return stateMachine;
        }
    }
}
