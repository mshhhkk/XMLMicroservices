#nullable enable
using FileParserService.App.Ports;
using Microsoft.Extensions.Options;
using Shared.Options;

namespace FileParserService.Infrastructure.FileSystem;

public class DirectoryFileEnumerator : IFileEnumerator
{
    private readonly string _folder;

    public DirectoryFileEnumerator(IOptions<WatchOptions> opt)
    {
        _folder = opt.Value.IncomeFolder;
        Directory.CreateDirectory(_folder);
    }

    public IEnumerable<string> Enumerate()
        => Directory.EnumerateFiles(_folder, "*.xml", SearchOption.TopDirectoryOnly);
}
