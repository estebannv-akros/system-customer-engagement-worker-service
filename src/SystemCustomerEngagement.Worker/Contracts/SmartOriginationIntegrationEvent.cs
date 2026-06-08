using AppMicroserviceCustomerEngagement.Application.Models;
using MassTransit;

namespace app.microservice.customer.engagement.worker.Contracts;

[EntityName("smart-origination-integration-event")]
public record SmartOriginationIntegrationEvent : HubSpotContact
{

}
