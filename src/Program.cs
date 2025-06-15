using KafkaWorker.Application;
using KafkaWorker.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddWorkerServices(builder.Configuration);

var host = builder.Build();
host.Run();