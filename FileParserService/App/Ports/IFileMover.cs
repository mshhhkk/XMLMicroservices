namespace FileParserService.App.Ports;
public interface IFileMover
{
    void MarkSuccess(string path);
    void MarkFailure(string path, Exception ex);
}
