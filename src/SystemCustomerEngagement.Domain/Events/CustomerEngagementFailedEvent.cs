using SystemCustomerEngagement.Domain.Common;
using SystemCustomerEngagement.Domain.ValueObjects;

namespace SystemCustomerEngagement.Domain.Events;

public sealed record CustomerEngagementFailedEvent(
    Guid EngagementId,
    CustomerId CustomerId,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
