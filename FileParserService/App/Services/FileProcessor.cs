using FileParserService.App.Ports;
using Shared.Contracts;

namespace FileParserService.App.Services;
public class FileProcessor : IFileProcessor
{
    private readonly IXmlParser _parser;
    private readonly IStateMutator _mutator;
    private readonly IRabbitMqService _publisher; 
    private readonly IFileMover _mover;
    private readonly ILogger<FileProcessor> _log;

    public FileProcessor(
        IXmlParser parser,
        IStateMutator mutator,
        IRabbitMqService publisher,
        IFileMover mover,
        ILogger<FileProcessor> log)
    {
        _parser = parser;
        _mutator = mutator;
        _publisher = publisher;
        _mover = mover;
        _log = log;
    }

    public async Task ProcessAsync(string path, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var (deviceId, modules) = await _parser.ParseAsync(path, ct);

            List<ModuleDTO> mutated = modules.Select(_mutator.Mutate).ToList();

            var msg = new DeviceMessage(deviceId, DateTimeOffset.UtcNow, mutated);
            _publisher.SendMessage(msg);

            _mover.MarkSuccess(path);
            _log.LogInformation("Processed {File}: {Count} modules, {Ms} ms",
                Path.GetFileName(path), mutated.Count, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _mover.MarkFailure(path, ex);
            _log.LogError(ex, "Failed to process {File}", path);
        }
    }
}
