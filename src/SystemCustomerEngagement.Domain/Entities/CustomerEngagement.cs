using SystemCustomerEngagement.Domain.Common;
using SystemCustomerEngagement.Domain.Events;
using SystemCustomerEngagement.Domain.ValueObjects;

namespace SystemCustomerEngagement.Domain.Entities;

public sealed class CustomerEngagement : AggregateRoot
{
    public CustomerId CustomerId { get; private set; }
    public EngagementChannel Channel { get; private set; }
    public EngagementStatus Status { get; private set; }
    public string Message { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private CustomerEngagement() { }

    public static CustomerEngagement Create(CustomerId customerId, EngagementChannel channel, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var engagement = new CustomerEngagement
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Channel = channel,
            Status = EngagementStatus.Pending,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        engagement.RaiseDomainEvent(new CustomerEngagementCreatedEvent(engagement.Id, customerId));
        return engagement;
    }

    public void MarkAsProcessed()
    {
        if (Status != EngagementStatus.Pending)
            throw new InvalidOperationException($"Cannot process engagement in status {Status}.");

        Status = EngagementStatus.Processed;
        ProcessedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CustomerEngagementProcessedEvent(Id, CustomerId));
    }

    public void MarkAsFailed(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = EngagementStatus.Failed;
        RaiseDomainEvent(new CustomerEngagementFailedEvent(Id, CustomerId, reason));
    }
}

public enum EngagementChannel { Email, Sms, Push }
public enum EngagementStatus { Pending, Processed, Failed }
