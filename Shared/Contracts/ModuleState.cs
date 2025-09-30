using System.Text.Json.Serialization;

namespace Shared.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModuleState { Online, Run, NotReady, Offline }
