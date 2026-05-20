namespace SystemCustomerEngagement.Domain.Common;

public abstract class AggregateRoot : Entity
{
    public int Version { get; protected set; }
}
