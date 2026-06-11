using AppMicroserviceCustomerEngagement.Application.UseCases;
using AppMicroserviceCustomerEngagement.Worker.Extensions;
using MassTransit;
using app.microservice.customer.engagement.worker.Contracts;

namespace app.microservice.customer.engagement.worker.Consumers;

public sealed class UserOriginationConsumer(
    UserOriginationIntegrationEventHandler handler,
    ILogger<UserOriginationConsumer> logger) : IConsumer<Batch<UserOriginationIntegrationEvent>>
{
    public async Task Consume(ConsumeContext<Batch<UserOriginationIntegrationEvent>> context)
    {
        var contacts = context.Message
            .Select(messageContext => messageContext.Message.ToHubSpotContact())
            .ToList();

        if (contacts.Count == 0)
        {
            logger.LogInformation("Batch recibido sin mensajes válidos, se omite llamada a HubSpot.");
            return;
        }

        logger.LogInformation("Procesando batch hacia HubSpot. Count={Count}", contacts.Count);

        await handler.ExecuteAsync(contacts, context.CancellationToken);

        logger.LogInformation("Batch enviado a HubSpot. Count={Count}", contacts.Count);
    }
}
