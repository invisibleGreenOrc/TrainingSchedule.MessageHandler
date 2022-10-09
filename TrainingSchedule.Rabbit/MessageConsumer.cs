using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using TrainingSchedule.Contracts;
using TrainingSchedule.Domain;

namespace TrainingSchedule.Rabbit
{
    public class MessageConsumer : IMessageReceiver
    {
        public event Func<MessageFromUser, Task>? MessageReceived;

        private ConnectionFactory _connectionFactory;

        public MessageConsumer()
        {
            _connectionFactory = new ConnectionFactory { HostName = "localhost" };
        }

        public void Start()
        {
            var connection = _connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "fromUserMessages", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<MessageFromUser>(body);

                if (message is not null)
                {
                    MessageReceived?.Invoke(message);
                }
            };

            channel.BasicConsume(queue: "fromUserMessages", autoAck: true, consumer: consumer);
        }
    }
}
