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

        if (string.IsNullOrWhiteSpace(msg.PasoActual))
            throw new PermanentException("El campo PasoActual es requerido.");

        logger.LogInformation(
            "Procesando engagement. CustomerId={CustomerId} Email={Email} Channel={Channel} PasoActual={PasoActual}",
            msg.CustomerId, msg.Email, msg.Channel, msg.PasoActual);

        await hubSpotClient.UpsertContactAsync(
            msg.Email,
            msg.PasoActual,
            context.CancellationToken);

        logger.LogInformation(
            "Contacto actualizado en HubSpot. Email={Email} PasoActual={PasoActual}",
            msg.Email, msg.PasoActual);

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
