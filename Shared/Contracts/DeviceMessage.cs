using System.Text.Json.Serialization;

namespace Shared.Contracts;

public record DeviceMessage(
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("modules")] IReadOnlyList<ModuleDTO> Modules
);
