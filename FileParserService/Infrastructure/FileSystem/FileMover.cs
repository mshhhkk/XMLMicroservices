#nullable enable
using FileParserService.App.Ports;
using Microsoft.Extensions.Options;
using Shared.Options;

namespace FileParserService.Infrastructure.FileSystem;

public class FileMover : IFileMover
{
    private readonly string _failFolder;
    private readonly ILogger<FileMover> _log;

    public FileMover(IOptions<WatchOptions> opt, ILogger<FileMover> log)
    {
        _failFolder = opt.Value.FailedFolder;
        Directory.CreateDirectory(_failFolder);
        _log = log;
    }

    public void MarkSuccess(string path)
    {
        try
        {
            File.Delete(path);
            _log.LogInformation("Deleted processed file {File}", Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to delete {File}", path);
        }
    }

    public void MarkFailure(string path, Exception _)
    {
        try
        {
            var dest = Path.Combine(_failFolder, Path.GetFileName(path));
            File.Move(path, dest, overwrite: true);
            _log.LogInformation("Moved fail file {File} → {Dest}", path, dest);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to move fail file {File}", path);
        }
    }
}
