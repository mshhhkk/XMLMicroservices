using System.Text.Json.Serialization;

namespace Shared.Contracts;
public record ModuleDTO(
    [property: JsonPropertyName("moduleCategoryId")] string ModuleCategoryId,
    [property: JsonPropertyName("moduleName")] string ModuleName,
    [property: JsonPropertyName("moduleState")] ModuleState ModuleState
);
