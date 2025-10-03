#nullable enable
using DataProcessorService.App.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _log;
    private readonly IMessageSubscriber _subscriber;
    private readonly IDeviceMessageHandler _handler;
    private readonly IModuleStateRepository _repo;

    public Worker(
        ILogger<Worker> log,
        IMessageSubscriber subscriber,
        IDeviceMessageHandler handler,
        IModuleStateRepository repo)
    {
        _log = log; _subscriber = subscriber; _handler = handler; _repo = repo;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            await _repo.EnsureCreatedAsync(ct);
            _log.LogInformation("DB ensured. Starting subscription...");

            await _subscriber.SubscribeAsync(
                onMessage: msg => _handler.HandleAsync(msg, ct),
                ct: ct);
        }
        catch (OperationCanceledException)
        {
            _log.LogInformation("Worker stopping (cancellation).");
        }
        finally
        {
            await _subscriber.DisposeAsync();
            _log.LogInformation("Worker stopped.");
        }
    }
}
