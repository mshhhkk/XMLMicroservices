using FileParserService.App.Ports;
using FileParserService.App.Services;
using FileParserService.Infrastructure.FileSystem;
using FileParserService.Infrastructure.Messaging;
using FileParserService.Infrastructure.Processing;
using FileParserService.Infrastructure.Xml;
using FileParserService.Workers;
using Shared.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);


builder.Services.Configure<WatchOptions>(builder.Configuration.GetSection("Watch"));
builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("RabbitMQ"));


builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<IFileEnumerator, DirectoryFileEnumerator>();
builder.Services.AddSingleton<IFileMover, FileMover>();
builder.Services.AddSingleton<IXmlParser, LinqXmlParser>();
builder.Services.AddSingleton<IStateMutator, RandomStateMutator>();
builder.Services.AddSingleton<IFileProcessor, FileProcessor>();

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
