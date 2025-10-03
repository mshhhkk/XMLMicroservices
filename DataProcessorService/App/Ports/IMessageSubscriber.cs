#nullable enable
using Shared.Contracts;

namespace DataProcessorService.App.Ports;
public interface IMessageSubscriber : IAsyncDisposable
{
    Task SubscribeAsync(Func<DeviceMessage, Task> onMessage, CancellationToken ct);
}
