using MassTransit;
using SystemCustomerEngagement.Application.Interfaces;
using SystemCustomerEngagement.Contracts.V1;
using SystemCustomerEngagement.Domain.Entities;
using SystemCustomerEngagement.Domain.Exceptions;

namespace SystemCustomerEngagement.Worker.Consumers;

public sealed class CustomerEngagementConsumer(
    IHubSpotClient hubSpotClient,
    ILogger<CustomerEngagementConsumer> logger) : IConsumer<CustomerEngagementRequested>
{
    public async Task Consume(ConsumeContext<CustomerEngagementRequested> context)
    {
        var msg = context.Message;

        if (!Enum.TryParse<EngagementChannel>(msg.Channel, ignoreCase: true, out _))
            throw new PermanentException($"Canal desconocido: '{msg.Channel}'. Valores válidos: Email, Sms, Push, InApp.");

        if (string.IsNullOrWhiteSpace(msg.Email))
            throw new PermanentException("El campo Email es requerido para identificar el contacto en HubSpot.");

        if (string.IsNullOrWhiteSpace(msg.CurrentStep))
            throw new PermanentException("El campo CurrentStep es requerido.");

        logger.LogInformation(
            "Procesando engagement. CustomerId={CustomerId} Email={Email} Channel={Channel} CurrentStep={CurrentStep}",
            msg.CustomerId, msg.Email, msg.Channel, msg.CurrentStep);

        await hubSpotClient.UpsertContactAsync(
            msg.Email,
            msg.CurrentStep,
            context.CancellationToken);

        logger.LogInformation(
            "Contacto actualizado en HubSpot. Email={Email} CurrentStep={CurrentStep}",
            msg.Email, msg.CurrentStep);

        // TODO: descomentar cuando se integre persistencia
        // var channel = Enum.Parse<EngagementChannel>(msg.Channel, ignoreCase: true);
        // var engagementId = await createHandler.HandleAsync(
        //     new CreateEngagementCommand(msg.CustomerId, channel, msg.Message),
        //     context.CancellationToken);
        // await processHandler.HandleAsync(
        //     new ProcessEngagementCommand(engagementId),
        //     context.CancellationToken);
    }
}
