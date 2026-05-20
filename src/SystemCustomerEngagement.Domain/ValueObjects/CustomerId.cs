using SystemCustomerEngagement.Domain.Common;
using SystemCustomerEngagement.Domain.Exceptions;

namespace SystemCustomerEngagement.Domain.ValueObjects;

public sealed class CustomerId : ValueObject
{
    public Guid Value { get; }

    private CustomerId(Guid value) => Value = value;

    public static CustomerId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("CustomerId cannot be empty.");
        return new CustomerId(value);
    }

    public static implicit operator Guid(CustomerId customerId) => customerId.Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
