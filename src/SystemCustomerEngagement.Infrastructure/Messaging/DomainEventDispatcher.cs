using MassTransit;
using Microsoft.Extensions.Logging;
using SystemCustomerEngagement.Application.Interfaces;
using SystemCustomerEngagement.Domain.Common;

namespace SystemCustomerEngagement.Infrastructure.Messaging;

public sealed class DomainEventDispatcher(
    IPublishEndpoint publishEndpoint,
    ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            logger.LogInformation(
                "Dispatching domain event {EventType} | EventId: {EventId} | OccurredOn: {OccurredOn}",
                domainEvent.GetType().Name,
                domainEvent.EventId,
                domainEvent.OccurredOn);

            await publishEndpoint.Publish((object)domainEvent, cancellationToken);
        }
    }
}
