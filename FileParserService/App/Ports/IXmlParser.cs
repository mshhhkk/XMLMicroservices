using Shared.Contracts;

namespace FileParserService.App.Ports;

public interface IXmlParser
{
    Task<(string deviceId, List<ModuleDTO> modules)> ParseAsync(string path, CancellationToken ct);
}
