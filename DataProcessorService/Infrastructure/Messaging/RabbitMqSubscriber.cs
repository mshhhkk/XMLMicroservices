#nullable enable
using System.Text;
using System.Text.Json;
using DataProcessorService.App.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;
using Shared.Options;
using Rmq = global::RabbitMQ.Client;

namespace DataProcessorService.Infra.Messaging;

public class RabbitMqSubscriber : IMessageSubscriber
{
    private readonly ILogger<RabbitMqSubscriber> _log;
    private readonly RabbitOptions _mq;
    private Rmq.IConnection? _conn;
    private Rmq.IModel? _ch;

    public RabbitMqSubscriber(IOptions<RabbitOptions> mq, ILogger<RabbitMqSubscriber> log)
    {
        _mq = mq.Value; _log = log;
    }

    public Task SubscribeAsync(Func<DeviceMessage, Task> onMessage, CancellationToken ct)
    {
        var factory = new Rmq.ConnectionFactory
        {
            HostName = _mq.HostName,
            Port = _mq.Port,
            UserName = _mq.UserName,
            Password = _mq.Password,
            VirtualHost = "/"
        };

        _conn = factory.CreateConnection();        
        _ch = _conn.CreateModel();

        _ch.ExchangeDeclare(_mq.Exchange, ExchangeType.Topic, durable: true);
        _ch.QueueDeclare(_mq.Queue, durable: true, exclusive: false, autoDelete: false);
        _ch.QueueBind(_mq.Queue, _mq.Exchange, _mq.RoutingKey);
        _ch.BasicQos(0, (ushort)_mq.Prefetch, false);

        var consumer = new EventingBasicConsumer(_ch);
        consumer.Received += async (_, ea) =>
        {
            var msgId = ea.BasicProperties?.MessageId ?? "-";
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<DeviceMessage>(json)!;

                await onMessage(msg);
                _ch.BasicAck(ea.DeliveryTag, false);
                _log.LogInformation("ACK {MsgId} ({Count} modules)", msgId, msg.Modules.Count);
            }
            catch (JsonException ex)
            {
                _log.LogWarning(ex, "Bad JSON; drop. MsgId={MsgId}", msgId);
                _ch!.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
            catch (OperationCanceledException)
            {
                _log.LogInformation("Cancellation; requeue. MsgId={MsgId}", msgId);
                _ch!.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Processing error; requeue. MsgId={MsgId}", msgId);
                _ch!.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        _ch.BasicConsume(_mq.Queue, autoAck: false, consumer);
        _log.LogInformation("Subscribed: ex={Ex}, q={Q}, key={Key}", _mq.Exchange, _mq.Queue, _mq.RoutingKey);

        return Task.Run(async () =>
        {
            try { await Task.Delay(Timeout.Infinite, ct); }
            catch (OperationCanceledException) { }
        }, ct);
    }

    public ValueTask DisposeAsync()
    {
        try { _ch?.Close(); } catch { }
        try { _conn?.Close(); } catch { }
        _ch?.Dispose();
        _conn?.Dispose();
        return ValueTask.CompletedTask;
    }
}
