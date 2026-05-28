namespace SystemCustomerEngagement.Worker.Contracts;

public record UpdateUserIntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Guid CustomerId { get; init; }
    public string Email { get; init; } = default!;
    public string Channel { get; init; } = default!;
    public string CurrentStep { get; init; } = default!;
    public string Message { get; init; } = default!;
}
