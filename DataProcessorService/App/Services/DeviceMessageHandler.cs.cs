#nullable enable
using DataProcessorService.App.Ports;
using Microsoft.Extensions.Logging;

namespace DataProcessorService.App.Services;

public class DeviceMessageHandler : IDeviceMessageHandler
{
    private readonly IModuleStateRepository _repo;
    private readonly ILogger<DeviceMessageHandler> _log;

    public DeviceMessageHandler(IModuleStateRepository repo, ILogger<DeviceMessageHandler> log)
    {
        _repo = repo; _log = log;
    }
    public async Task HandleAsync(Shared.Contracts.DeviceMessage message, CancellationToken ct)
    {
        var rows = message.Modules.Select(m => (m.ModuleCategoryId, m.ModuleState, DateTimeOffset.UtcNow));
        await _repo.UpsertManyAsync(rows, ct);
        _log.LogInformation("Saved {Count} module states for device {Device}", message.Modules.Count, message.DeviceId);
    }
}
