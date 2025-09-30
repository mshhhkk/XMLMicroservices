namespace FileParserService.App.Ports;
public interface IFileProcessor
{
    Task ProcessAsync(string path, CancellationToken ct);
}
