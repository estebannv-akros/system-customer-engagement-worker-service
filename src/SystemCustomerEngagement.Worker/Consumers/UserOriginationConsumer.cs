using MassTransit;
using AppMicroserviceCustomerEngagement.Application.Interfaces;
using app.microservice.customer.engagement.worker.Contracts.UserOrigination;

namespace app.microservice.customer.engagement.worker.Consumers;

public sealed class UserOriginationConsumer(
    IHubSpotServiceProvider hubspotServiceProvider,
    ILogger<UserOriginationConsumer> logger) : IConsumer<Batch<UserOriginationIntegrationEvent>>
{
    public async Task Consume(ConsumeContext<Batch<UserOriginationIntegrationEvent>> context)
    {
        var contacts = UserOriginationIntegrationEventMapper.ToHubSpotContacts(
            context.Message.Select(m => m.Message), logger);

        if (contacts.Count == 0)
        {
            logger.LogInformation("Batch recibido sin mensajes válidos, se omite llamada a HubSpot.");
            return;
        }

        logger.LogInformation("Procesando batch hacia HubSpot. Count={Count}", contacts.Count);

        await hubspotServiceProvider.UpsertContactsBatchAsync(contacts, context.CancellationToken);

        logger.LogInformation("Batch enviado a HubSpot. Count={Count}", contacts.Count);
    }
}
