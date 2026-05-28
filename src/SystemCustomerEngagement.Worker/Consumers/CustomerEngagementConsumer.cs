using MassTransit;
using SystemCustomerEngagement.Domain.Interfaces;
using SystemCustomerEngagement.Worker.Contracts;
using SystemCustomerEngagement.Domain.Entities;
using SystemCustomerEngagement.Domain.Exceptions;

namespace SystemCustomerEngagement.Worker.Consumers;

public sealed class CustomerEngagementConsumer(
    IHubSpotClient hubSpotClient,
    ILogger<CustomerEngagementConsumer> logger) : IConsumer<Batch<CustomerEngagementRequested>>
{
    public async Task Consume(ConsumeContext<Batch<CustomerEngagementRequested>> context)
    {
        var valid = new List<(string Email, string CurrentStep)>();

        foreach (var item in context.Message)
        {
            var msg = item.Message;

            if (!Enum.TryParse<EngagementChannel>(msg.Channel, ignoreCase: true, out _))
            {
                logger.LogWarning(
                    "Mensaje descartado — canal desconocido: '{Channel}'. CustomerId={CustomerId}",
                    msg.Channel, msg.CustomerId);
                continue;
            }

            if (string.IsNullOrWhiteSpace(msg.Email) || string.IsNullOrWhiteSpace(msg.CurrentStep))
            {
                logger.LogWarning(
                    "Mensaje descartado — Email o CurrentStep vacío. CustomerId={CustomerId}",
                    msg.CustomerId);
                continue;
            }

            valid.Add((msg.Email, msg.CurrentStep));
        }

        if (valid.Count == 0)
        {
            logger.LogInformation("Batch recibido sin mensajes válidos, se omite llamada a HubSpot.");
            return;
        }

        logger.LogInformation(
            "Procesando batch hacia HubSpot. Count={Count}",
            valid.Count);

        await hubSpotClient.UpsertContactsBatchAsync(valid, context.CancellationToken);

        logger.LogInformation(
            "Batch enviado a HubSpot. Count={Count}",
            valid.Count);

        // TODO: descomentar cuando se integre persistencia
        // foreach (var (email, step) in valid) { ... }
    }
}
