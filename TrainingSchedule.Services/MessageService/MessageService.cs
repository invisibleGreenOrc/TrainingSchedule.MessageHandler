using TrainingSchedule.Domain;
using TrainingSchedule.Services.CommandHandlers;
using TrainingSchedule.Services.FSM;

namespace TrainingSchedule.Services.MessageService
{
    public class MessageService
    {
        private readonly IBotClient _botClient;

        private FiniteStateMachineBuilder _stateMachineBuilder;

        private Dictionary<long, FiniteStateMachine> _usersStateMachines = new();

        public MessageService(IBotClient botClient, IEnumerable<ICommandHandler> commandHandlers)
        {
            _botClient = botClient;
            _botClient.MessageReceived += HandleMessageAsync;

            _stateMachineBuilder = new FiniteStateMachineBuilder(commandHandlers);
        }

        public async Task HandleMessageAsync(long botUserId, long chatId, string message)
        {
            await GetStateMachineForUser(botUserId).ProcessMessage(botUserId, chatId, message);
        }

        private FiniteStateMachine GetStateMachineForUser(long botUserId)
        {
            if (_usersStateMachines.TryGetValue(botUserId, out FiniteStateMachine? fsm) && (fsm is not null))
            {
                return fsm;
            }

            fsm = _stateMachineBuilder.Build();
            _usersStateMachines[botUserId] = fsm;

            return fsm;
        }
    }
}