using Shared.Contracts;

namespace DataProcessorService.App.Ports;

public interface IModuleStateRepository
{
    Task EnsureCreatedAsync(CancellationToken ct);
    Task UpsertManyAsync(
        IEnumerable<(string ModuleCategoryId, ModuleState State, DateTimeOffset UpdatedAt)> rows,
        CancellationToken ct);
}