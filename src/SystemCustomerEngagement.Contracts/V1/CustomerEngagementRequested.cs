namespace SystemCustomerEngagement.Contracts.V1;

/// <summary>
/// Comando de mensajería para solicitar el envío de una comunicación a un cliente.
/// Channel: "Email" | "Sms" | "Push" | "InApp"
/// </summary>
public record CustomerEngagementRequested
{
    public Guid CorrelationId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Guid CustomerId { get; init; }
    public string Channel { get; init; } = default!;
    public string Message { get; init; } = default!;
}
