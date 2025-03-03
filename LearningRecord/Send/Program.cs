using RabbitMQ.Client;
using System;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

Console.WriteLine("链接成功，请输入消息，输入 exit 退出");

string input;
do
{
    input = Console.ReadLine();
    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(input);
    var props = new BasicProperties();
    props.ContentType = "text/plain";
    props.DeliveryMode = DeliveryModes.Persistent;

    await channel.BasicPublishAsync(string.Empty, "hello",
        mandatory: true, basicProperties: props, body: messageBodyBytes);
    Console.WriteLine($" [x] Sent {input}");
} while (input.ToLower() != "exit");

