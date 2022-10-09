using Microsoft.Extensions.Hosting;
using TrainingSchedule.Contracts;
using TrainingSchedule.Domain;
using TrainingSchedule.Services.CommandHandlers;
using TrainingSchedule.Services.FSM;

namespace TrainingSchedule.Services
{
    public class MessageProcessingService : IHostedService
    {
        private readonly IMessageReceiver _messageReceiver;

        private FiniteStateMachineBuilder _stateMachineBuilder;

        private Dictionary<long, FiniteStateMachine> _usersStateMachines = new();

        public MessageProcessingService(IMessageReceiver messageReceiver, IEnumerable<ICommandHandler> commandHandlers)
        {
            _messageReceiver = messageReceiver;
            _messageReceiver.MessageReceived += HandleMessageAsync;

            _stateMachineBuilder = new FiniteStateMachineBuilder(commandHandlers);
        }

        private async Task HandleMessageAsync(MessageFromUser message)
        {
            await GetStateMachineForUser(message.BotUserId).ProcessMessage(message.BotUserId, message.ChatId, message.Body);
        }

        private FiniteStateMachine GetStateMachineForUser(long botUserId)
        {
            if (_usersStateMachines.TryGetValue(botUserId, out FiniteStateMachine? fsm) && fsm is not null)
            {
                return fsm;
            }

            fsm = _stateMachineBuilder.Build();
            _usersStateMachines[botUserId] = fsm;

            return fsm;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _messageReceiver.Start();
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
        }
    }
}