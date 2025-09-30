#nullable enable
using FileParserService.App.Ports;
using Shared.Contracts;
using System.Xml.Linq;

namespace FileParserService.Infrastructure.Xml;

public class LinqXmlParser : IXmlParser
{
    private readonly ILogger<LinqXmlParser> _log;

    public LinqXmlParser(ILogger<LinqXmlParser> log) => _log = log;

    public Task<(string deviceId, List<ModuleDTO> modules)> ParseAsync(string path, CancellationToken ct)
    {
        var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);

        var deviceId = doc.Root?.Element("PackageID")?.Value?.Trim() ?? "UNKNOWN";
        var modules = new List<ModuleDTO>();

        foreach (var ds in doc.Root?.Elements("DeviceStatus") ?? Enumerable.Empty<XElement>())
        {
            ct.ThrowIfCancellationRequested();

            var categoryId = ds.Element("ModuleCategoryID")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(categoryId)) continue;

            var rcs = ds.Element("RapidControlStatus");
            string? stateText = null;

            var innerRoot = rcs?.Elements().FirstOrDefault();
            if (innerRoot != null)
            {
                stateText = innerRoot.Element("ModuleState")?.Value?.Trim();
            }
            else if (rcs != null)
            {
                var raw = string.Concat(rcs.Nodes().OfType<XText>().Select(t => t.Value)).Trim();
                if (!string.IsNullOrEmpty(raw))
                {
                    var cleaned = RemoveXmlDeclaration(raw);
                    try
                    {
                        var inner = XDocument.Parse(cleaned);
                        stateText = inner.Root?.Element("ModuleState")?.Value?.Trim();
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Cannot parse inner RapidControlStatus XML in {File}", path);
                    }
                }
            }

            var state = ModuleState.NotReady;
            if (!string.IsNullOrEmpty(stateText) && Enum.TryParse(stateText, true, out ModuleState parsed))
                state = parsed;

            modules.Add(new ModuleDTO(categoryId!, categoryId!, state));
        }

        return Task.FromResult((deviceId, modules));
    }

    private static string RemoveXmlDeclaration(string s)
    {
        var idxStart = s.IndexOf("<?xml", StringComparison.OrdinalIgnoreCase);
        if (idxStart >= 0)
        {
            var idxEnd = s.IndexOf("?>", idxStart, StringComparison.OrdinalIgnoreCase);
            if (idxEnd > idxStart) return s.Remove(idxStart, (idxEnd - idxStart) + 2);
        }
        return s;
    }
}
