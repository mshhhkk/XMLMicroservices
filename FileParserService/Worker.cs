#nullable enable
using FileParserService.App.Ports;
using Microsoft.Extensions.Options;
using Shared.Options;

namespace FileParserService.Workers;

public class Worker : BackgroundService
{
    private readonly IFileEnumerator _enumerator;
    private readonly IFileProcessor _processor;
    private readonly ILogger<Worker> _log;
    private readonly int _intervalMs;
    private readonly SemaphoreSlim _sem;

    public Worker(
        IFileEnumerator enumerator,
        IFileProcessor processor,
        IOptions<WatchOptions> opt,
        ILogger<Worker> log)
    {
        _enumerator = enumerator;
        _processor = processor;
        _intervalMs = Math.Max(200, opt.Value.IntervalMs);
        _sem = new SemaphoreSlim(Math.Max(1, opt.Value.MaxParallel));
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("FileScanWorker started with interval {Ms} ms", _intervalMs);

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_intervalMs));

        while (await timer.WaitForNextTickAsync(ct))
        {
            foreach (var file in _enumerator.Enumerate())
            {
                await _sem.WaitAsync(ct);

                _ = Task.Run(async () =>
                {
                    try { await _processor.ProcessAsync(file, ct); }
                    catch (OperationCanceledException) { _log.LogInformation("Cancellation requested; stop processing {File}", file); }
                    catch (Exception ex) { _log.LogError(ex, "Unhandled error for {File}", file); }
                    finally { _sem.Release(); }
                }, ct);
            }
        }
    }
}
