using MassTransit;

namespace app.microservice.customer.engagement.worker.Contracts.UserOrigination;

[EntityName("user-origination-integration-event")]
public record UserOriginationIntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Guid CustomerId { get; init; }
    public string Email { get; init; } = default!;
    public string Channel { get; init; } = default!;
    public string CurrentStep { get; init; } = default!;
    public string Message { get; init; } = default!;
}
