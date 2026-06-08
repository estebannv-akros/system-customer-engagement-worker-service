using AppMicroserviceCustomerEngagement.Application.Models;
using MassTransit;

namespace app.microservice.customer.engagement.worker.Contracts;

[EntityName("credit-origination-integration-event")]
public record CreditOriginationIntegrationEvent: HubSpotContact
{

}
