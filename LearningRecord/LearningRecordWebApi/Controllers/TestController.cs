using Microsoft.AspNetCore.Hosting.Server;
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
        public IWebHostEnvironment webHostEnvironment;
        public TestController(IWebHostEnvironment environment)
        {
            webHostEnvironment = environment;
        }

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

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            ResponseContent response = new ResponseContent();
            try
            {
                // 在服务器端，应该验证上传文件的类型，防止恶意文件（如可执行文件）的上传。
                // 可以通过检查文件的扩展名或者内容类型（ContentType属性）来进行验证
                var allowedExtensions = new[] { ".jpg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(fileExtension))
                {
                    response.Success = false;
                    response.Message = "不允许的文件类型";
                    return StatusCode(500, response);
                }
                if (file == null || file.Length == 0)
                {
                    response.Success = false;
                    response.Message = "没有文件上传";
                    return StatusCode(500, response);
                }
                // 文件大小限制 // 限制为10MB
                if (file.Length > 10 * 1024 * 1024)
                {
                    response.Success = false;
                    response.Message = "文件太大";
                    return StatusCode(500, response);
                }
                // 设置上传文件存放路径
                string webPath = string.Format("{0}{1}{2}{3}", webHostEnvironment.ContentRootPath, Path.DirectorySeparatorChar, "Upload", Path.DirectorySeparatorChar);

                if (!Directory.Exists(webPath))
                {
                    Directory.CreateDirectory(webPath);
                }
                // 
                using (FileStream filestream = System.IO.File.Create(webPath + file.FileName))
                {
                    file.CopyTo(filestream);
                    filestream.Flush();
                }
            }
            catch (Exception)
            {
                response.Success = false;
                response.Message = "文件上传失败";
                return StatusCode(500, response);
            }
            response.Success = false;
            response.Message = "上传成功";
            return Ok(response);
        }

        [HttpGet]
        public IActionResult Download() 
        {
            string filePath = string.Format("{0}{1}{2}{3}{4}", webHostEnvironment.ContentRootPath, Path.DirectorySeparatorChar, "Upload", Path.DirectorySeparatorChar,"1.jpg");
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            return File(fileStream, "application/octet-stream", "test.jpg");
        }

        private async Task<IChannel> CreactChannelAsync(string hostName)
        {
            var factory = new ConnectionFactory { HostName = hostName };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            return channel;
        }
    }
}
