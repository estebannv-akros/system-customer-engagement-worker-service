namespace AppMicroserviceCustomerEngagement.Application.Models;

public record HubSpotContact
{
    public string CorrelationId { get; init; } = default!;
    public DateTimeOffset Timestamp { get; init; }
    public string Email { get; init; } = default!;
    public string Message { get; init; } = default!;
    public int BrandId { get; init; }
}
