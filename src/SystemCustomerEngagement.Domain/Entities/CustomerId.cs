using SystemCustomerEngagement.Domain.Common;

namespace SystemCustomerEngagement.Domain.Entities;

public sealed class CustomerId : ValueObject
{
    public Guid Value { get; }

    private CustomerId(Guid value) => Value = value;

    public static CustomerId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(value));

        return new CustomerId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
