using AppMicroserviceCustomerEngagement.Application.Models;
using app.microservice.customer.engagement.worker.Contracts;

namespace AppMicroserviceCustomerEngagement.Worker.Extensions;

internal static class HubSpotContactMapper
{
    public static HubSpotContact ToHubSpotContact(this HubSpotIntegrationEvent integrationEvent) =>
        new()
        {
            CorrelationId = integrationEvent.CorrelationId,
            Timestamp = integrationEvent.Timestamp,
            Email = integrationEvent.Email,
            Message = integrationEvent.Message,
            BrandId = integrationEvent.BrandId,
        };
}
