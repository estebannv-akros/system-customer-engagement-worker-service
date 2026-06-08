namespace AppMicroserviceCustomerEngagement.Application.Models;

public record HubSpotContact
{
    public string CorrelationId { get; init; } = default!;
    public DateTimeOffset Timestamp { get; init; }
    public string CustomerId { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Channel { get; init; } = default!;
    public string CurrentStep { get; init; } = default!;
    public string Message { get; init; } = default!;
    public string Flow { get; init; } = default!;
}
