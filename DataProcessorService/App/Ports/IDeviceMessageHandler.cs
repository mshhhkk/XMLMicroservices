using Shared.Contracts;

namespace DataProcessorService.App.Ports;

public interface IDeviceMessageHandler
{
    Task HandleAsync(DeviceMessage message, CancellationToken ct);
}
