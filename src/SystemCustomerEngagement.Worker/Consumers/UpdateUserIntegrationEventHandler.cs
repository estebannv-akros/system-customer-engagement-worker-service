using MassTransit;
using AppMicroserviceCustomerEngagement.Infrastructure.HubSpot;
using AppMicroserviceCustomerEngagement.Worker.Contracts;

namespace AppMicroserviceCustomerEngagement.Worker.Consumers;

public sealed class UpdateUserIntegrationEventHandler(
    HubSpotServiceProvider hubspotServiceProvider,
    ILogger<UpdateUserIntegrationEventHandler> logger) : IConsumer<Batch<UpdateUserIntegrationEvent>>
{
    public async Task Consume(ConsumeContext<Batch<UpdateUserIntegrationEvent>> context)
    {
        var contacts = UpdateUserIntegrationEventMapper.ToHubSpotContacts(
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
