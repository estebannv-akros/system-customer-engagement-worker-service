using MassTransit;
using AppMicroserviceCustomerEngagement.Application.Interfaces;
using app.microservice.customer.engagement.worker.Contracts.SmartOrigination;

namespace app.microservice.customer.engagement.worker.Consumers;

public sealed class SmartOriginationConsumer(
    IHubSpotServiceProvider hubspotServiceProvider,
    ILogger<SmartOriginationConsumer> logger) : IConsumer<Batch<SmartOriginationIntegrationEvent>>
{
    public async Task Consume(ConsumeContext<Batch<SmartOriginationIntegrationEvent>> context)
    {
        var contacts = SmartOriginationIntegrationEventMapper.ToHubSpotContacts(
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
