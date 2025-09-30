namespace Shared.Options;

public class WatchOptions
{
    public string IncomeFolder { get; set; } = ".\\in";
    public string FailedFolder { get; set; } = ".\\fail";
    public int IntervalMs { get; set; } = 1000;
    public int MaxParallel { get; set; } = 4;
}
