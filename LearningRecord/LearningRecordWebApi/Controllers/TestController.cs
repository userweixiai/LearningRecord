using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace LearningRecordWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<string> SendAsync(string message) 
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = DeliveryModes.Persistent;

            await channel.BasicPublishAsync(string.Empty, "hello",
                mandatory: true, basicProperties: props, body: messageBodyBytes);

            return message;
        }
    }
}
