using DataProcessorService.App.Ports;
using DataProcessorService.App.Services;
using DataProcessorService.Infra.Messaging;
using DataProcessorService.Infra.Persistence;
using Microsoft.Extensions.Hosting;
using Shared.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<SqliteOptions>(builder.Configuration.GetSection("SQLite"));

builder.Services.AddSingleton<IModuleStateRepository, SqliteModuleStateRepository>();
builder.Services.AddSingleton<IMessageSubscriber, RabbitMqSubscriber>();
builder.Services.AddSingleton<IDeviceMessageHandler, DeviceMessageHandler>();

builder.Services.AddHostedService<Worker>();

var app = builder.Build();
await app.RunAsync();
