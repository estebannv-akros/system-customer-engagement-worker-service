using Microsoft.Extensions.Logging;
using SystemCustomerEngagement.Application.Interfaces;
using SystemCustomerEngagement.Domain.Common;

namespace SystemCustomerEngagement.Infrastructure.Messaging;

public sealed class DomainEventDispatcher(ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            logger.LogInformation(
                "Domain event dispatched: {EventType} | EventId: {EventId} | OccurredOn: {OccurredOn}",
                domainEvent.GetType().Name,
                domainEvent.EventId,
                domainEvent.OccurredOn);
        }

        return Task.CompletedTask;
    }
}
