namespace FileParserService.App.Ports;
public interface IRabbitMqService
{
    void SendMessage(object obj);
    void SendMessage(string message);
}