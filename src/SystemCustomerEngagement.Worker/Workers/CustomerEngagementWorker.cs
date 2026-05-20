using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SystemCustomerEngagement.Application.Commands;
using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Application.DTOs;
using SystemCustomerEngagement.Application.Queries;

namespace SystemCustomerEngagement.Worker.Workers;

public sealed class CustomerEngagementWorker(
    ILogger<CustomerEngagementWorker> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CustomerEngagementWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingEngagementsAsync(stoppingToken);
            await Task.Delay(PollingInterval, stoppingToken);
        }

        logger.LogInformation("CustomerEngagementWorker stopped.");
    }

    private async Task ProcessPendingEngagementsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var queryHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetPendingEngagementsQuery, IEnumerable<EngagementDto>>>();

        var commandHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<ProcessEngagementCommand>>();

        var pending = (await queryHandler.HandleAsync(new GetPendingEngagementsQuery(10), cancellationToken)).ToList();

        if (pending.Count == 0)
        {
            logger.LogDebug("No pending engagements found.");
            return;
        }

        logger.LogInformation("Processing {Count} pending engagements.", pending.Count);

        foreach (var engagement in pending)
        {
            try
            {
                await commandHandler.HandleAsync(new ProcessEngagementCommand(engagement.Id), cancellationToken);
                logger.LogInformation("Engagement {EngagementId} processed successfully.", engagement.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process engagement {EngagementId}.", engagement.Id);
            }
        }
    }
}
