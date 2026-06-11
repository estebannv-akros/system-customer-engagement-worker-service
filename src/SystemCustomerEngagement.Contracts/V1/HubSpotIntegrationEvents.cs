namespace app.microservice.customer.engagement.worker.Contracts;

public record HubSpotIntegrationEvent
{
    public string CorrelationId { get; init; } = default!;
    public DateTimeOffset Timestamp { get; init; }
    public string Email { get; init; } = default!;
    public string Message { get; init; } = default!;
    public int BrandId { get; init; }
}

public record CreditOriginationIntegrationEvent : HubSpotIntegrationEvent;

public record SmartOriginationIntegrationEvent : HubSpotIntegrationEvent;

public record UserOriginationIntegrationEvent : HubSpotIntegrationEvent;
