
namespace FileParserService.App.Ports;
public interface IFileEnumerator
{
    IEnumerable<string> Enumerate();
}
