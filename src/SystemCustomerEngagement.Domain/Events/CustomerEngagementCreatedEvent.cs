using SystemCustomerEngagement.Domain.Common;
using SystemCustomerEngagement.Domain.ValueObjects;

namespace SystemCustomerEngagement.Domain.Events;

public sealed record CustomerEngagementCreatedEvent(
    Guid EngagementId,
    CustomerId CustomerId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
