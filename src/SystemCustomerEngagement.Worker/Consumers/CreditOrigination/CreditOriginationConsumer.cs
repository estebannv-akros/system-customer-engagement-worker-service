using MassTransit;
using AppMicroserviceCustomerEngagement.Infrastructure.HubSpot;

namespace app.microservice.customer.engagement.worker.Consumers.CreditOrigination;

public sealed class CreditOriginationConsumer(
    HubSpotServiceProvider hubspotServiceProvider,
    ILogger<CreditOriginationConsumer> logger) : IConsumer<Batch<CreditOriginationIntegrationEvent>>
{
    public async Task Consume(ConsumeContext<Batch<CreditOriginationIntegrationEvent>> context)
    {
        var contacts = CreditOriginationIntegrationEventMapper.ToHubSpotContacts(
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
