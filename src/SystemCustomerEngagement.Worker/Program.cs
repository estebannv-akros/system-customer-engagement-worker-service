using DotNetEnv;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using SystemCustomerEngagement.Infrastructure.Extensions;
using SystemCustomerEngagement.Worker.Extensions;

Env.Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddMassTransitWithRabbitMq(builder.Configuration);

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("customer-engagement-worker"))
    .WithTracing(t => t
        .AddSource("MassTransit")
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]!);
            opts.Protocol = OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(m => m
        .AddMeter("MassTransit")
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]!);
            opts.Protocol = OtlpExportProtocol.Grpc;
        }));

var host = builder.Build();
host.Run();
