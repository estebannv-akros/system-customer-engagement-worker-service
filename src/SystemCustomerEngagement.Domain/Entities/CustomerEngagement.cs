using SystemCustomerEngagement.Domain.Common;

namespace SystemCustomerEngagement.Domain.Entities;

public sealed class CustomerEngagement : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public EngagementChannel Channel { get; private set; }
    public EngagementStatus Status { get; private set; }
    public string Message { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private CustomerEngagement() { }

    public static CustomerEngagement Create(CustomerId customerId, EngagementChannel channel, string message)
    {
        var engagement = new CustomerEngagement
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId.Value,
            Channel = channel,
            Status = EngagementStatus.Pending,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        engagement.RaiseDomainEvent(new EngagementCreatedEvent(engagement.Id, customerId.Value));
        return engagement;
    }

    public void MarkProcessed()
    {
        Status = EngagementStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        RaiseDomainEvent(new EngagementProcessedEvent(Id));
    }

    public void MarkFailed()
    {
        Status = EngagementStatus.Failed;
        RaiseDomainEvent(new EngagementFailedEvent(Id));
    }
}

public sealed record EngagementCreatedEvent(Guid EngagementId, Guid CustomerId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record EngagementProcessedEvent(Guid EngagementId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record EngagementFailedEvent(Guid EngagementId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
