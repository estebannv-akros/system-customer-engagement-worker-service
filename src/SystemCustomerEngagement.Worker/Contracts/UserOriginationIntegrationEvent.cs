using AppMicroserviceCustomerEngagement.Application.Models;
using MassTransit;

namespace app.microservice.customer.engagement.worker.Contracts;

[EntityName("user-origination-integration-event")]
public record UserOriginationIntegrationEvent : HubSpotContact
{

}
