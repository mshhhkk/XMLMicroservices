namespace Shared.Options;

public class RabbitOptions
{
    public string HostName { get; set; } 
    public int Port { get; set; } 
    public string UserName { get; set; } 
    public string Password { get; set; } 

    public string Exchange { get; set; }
    public string RoutingKey { get; set; } 
    public string Queue { get; set; } 

    public int Prefetch { get; set; } = 50;
}
