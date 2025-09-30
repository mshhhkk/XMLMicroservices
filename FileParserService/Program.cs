using FileParserService.App.Ports;
using FileParserService.App.Services;
using FileParserService.Infrastructure.FileSystem;
using FileParserService.Infrastructure.Messaging;
using FileParserService.Infrastructure.Processing;
using FileParserService.Infrastructure.Xml;
using FileParserService.Workers;
using Shared.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<WatchOptions>(ctx.Configuration.GetSection("Watch"));
        services.Configure<RabbitOptions>(ctx.Configuration.GetSection("RabbitMQ"));

        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddSingleton<IFileEnumerator, DirectoryFileEnumerator>();
        services.AddSingleton<IFileMover, FileMover>();
        services.AddSingleton<IXmlParser, LinqXmlParser>();
        services.AddSingleton<IStateMutator, RandomStateMutator>();
        services.AddSingleton<IFileProcessor, FileProcessor>();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();