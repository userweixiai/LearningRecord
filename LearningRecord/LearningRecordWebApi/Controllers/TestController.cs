using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace LearningRecordWebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<string> SendAsync(string message) 
        {
            var channel = await CreactChannelAsync("localhost");
            await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = DeliveryModes.Persistent;

            await channel.BasicPublishAsync(string.Empty, "hello",
                mandatory: true, basicProperties: props, body: messageBodyBytes);

            return message;
        }

        [HttpGet]
        public async Task<string> ReceiveAsync()
        {
            var channel = await CreactChannelAsync("localhost");
            await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var sb = new StringBuilder();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                sb.Append(message);
                // 确认消息已经被消费
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            // 启动消费者，设置为手动应答消息
            await channel.BasicConsumeAsync(queue: "hello",
                                 autoAck: false,
                                 consumer: consumer);
            var result = sb.ToString();
            return result;
        }

        public async Task<IChannel> CreactChannelAsync(string hostName) 
        {
            var factory = new ConnectionFactory { HostName = hostName };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            return channel;
        }
    }
}
