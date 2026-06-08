using MassTransit;

namespace app.microservice.customer.engagement.worker.Contracts.CreditOrigination;

[EntityName("credit-origination-integration-event")]
public record CreditOriginationIntegrationEvent
{
    public string CorrelationId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string ContactId { get; init; }
    public string Email { get; init; } = default!;
    public string Channel { get; init; } = default!;
    public string CurrentStep { get; init; } = default!;
    public string Message { get; init; } = default!;
}
