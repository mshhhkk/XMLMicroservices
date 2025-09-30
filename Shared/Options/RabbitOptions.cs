namespace Shared.Options;

public class RabbitOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public string Exchange { get; set; } = "modules.topic";
    public string RoutingKey { get; set; } = "modules.update";
    public string Queue { get; set; } = "modules.db";

    public int Prefetch { get; set; } = 50;
}
