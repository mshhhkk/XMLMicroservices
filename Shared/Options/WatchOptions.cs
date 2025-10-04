namespace Shared.Options;

public class WatchOptions
{
    public string IncomeFolder { get; set; }
    public string FailedFolder { get; set; }
    public int IntervalMs { get; set; }
    public int MaxParallel { get; set; }
}
