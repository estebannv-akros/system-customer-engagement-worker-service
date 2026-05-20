using SystemCustomerEngagement.Infrastructure.Extensions;
using SystemCustomerEngagement.Worker.Extensions;
using SystemCustomerEngagement.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddInfrastructure()
    .AddApplication()
    .AddHostedService<CustomerEngagementWorker>();

var host = builder.Build();
host.Run();
