using RabbitMQ.Client;
using System.Text.Json;
using TrainingSchedule.Contracts;
using TrainingSchedule.Domain;

namespace TrainingSchedule.Rabbit
{
    public class MessageProducer : IMessageSender
    {
        ConnectionFactory _connectionFactory;

        public MessageProducer()
        {
            _connectionFactory = new ConnectionFactory { HostName = "localhost" };
        }

        public Task SendAsync(long chatId, string messageBody, IAllowedAnswers? allowedAnswers = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "toUserMessages", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var messageTest = new MessageToUser
                    {
                        ChatId = chatId,
                        Message = messageBody,
                        AllowedAnswers = allowedAnswers
                    };

                    var encodedMessageTest = JsonSerializer.SerializeToUtf8Bytes(messageTest);

                    channel.BasicPublish("", "toUserMessages", null, encodedMessageTest);
                }
            }

            return Task.CompletedTask;
        }
    }
}
