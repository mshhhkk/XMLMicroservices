#nullable enable
using FileParserService.App.Ports;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Options;
using System.Text;
using System.Text.Json;
using Rmq = global::RabbitMQ.Client;

namespace FileParserService.Infrastructure.Messaging;
public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly ILogger<RabbitMqService> _log;
    private readonly RabbitOptions _opt;
    private readonly Rmq.IConnection _conn;
    private readonly Rmq.IModel _ch;

    public RabbitMqService(IOptions<RabbitOptions> opt, ILogger<RabbitMqService> log)
    {
        _opt = opt.Value; _log = log;

        var factory = new Rmq.ConnectionFactory
        {
            HostName = _opt.HostName,
            Port = _opt.Port,
            UserName = _opt.UserName,
            Password = _opt.Password,
            VirtualHost = "/"
        };

        _conn = factory.CreateConnection();
        _ch = _conn.CreateModel();

        _ch.ExchangeDeclare(_opt.Exchange, Rmq.ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
        _ch.QueueDeclare(_opt.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _ch.QueueBind(_opt.Queue, _opt.Exchange, _opt.RoutingKey);

        _log.LogInformation("Rabbit connected {Host}:{Port}, ex={Ex}, q={Q}, key={Key}",
            _opt.HostName, _opt.Port, _opt.Exchange, _opt.Queue, _opt.RoutingKey);
    }

    public void SendMessage(object obj) => SendMessage(JsonSerializer.Serialize(obj));

    public void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var props = _ch.CreateBasicProperties();
        props.Persistent = true;

        _ch.BasicPublish(exchange: _opt.Exchange, routingKey: _opt.RoutingKey, basicProperties: props, body: body);
        _log.LogInformation("Published {len} bytes", body.Length);
    }

    public void Dispose()
    {
        try { _ch?.Close(); } catch { }
        try { _conn?.Close(); } catch { }
        _ch?.Dispose();
        _conn?.Dispose();
    }
}
