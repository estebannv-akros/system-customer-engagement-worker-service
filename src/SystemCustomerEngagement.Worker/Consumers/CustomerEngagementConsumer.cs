using MassTransit;
using SystemCustomerEngagement.Application.Commands;
using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Contracts.V1;
using SystemCustomerEngagement.Domain.Entities;
using SystemCustomerEngagement.Domain.Exceptions;

namespace SystemCustomerEngagement.Worker.Consumers;

public sealed class CustomerEngagementConsumer(
    ICommandHandler<CreateEngagementCommand, Guid> createHandler,
    ICommandHandler<ProcessEngagementCommand> processHandler,
    ILogger<CustomerEngagementConsumer> logger) : IConsumer<CustomerEngagementRequested>
{
    public async Task Consume(ConsumeContext<CustomerEngagementRequested> context)
    {
        var msg = context.Message;

        if (!Enum.TryParse<EngagementChannel>(msg.Channel, ignoreCase: true, out var channel))
            throw new PermanentException($"Canal desconocido: '{msg.Channel}'. Valores válidos: Email, Sms, Push, InApp.");

        logger.LogInformation(
            "Creando engagement para CustomerId={CustomerId} Channel={Channel}",
            msg.CustomerId, channel);

        Guid engagementId;
        try
        {
            engagementId = await createHandler.HandleAsync(
                new CreateEngagementCommand(msg.CustomerId, channel, msg.Message),
                context.CancellationToken);
        }
        catch (ArgumentException ex)
        {
            throw new PermanentException("Payload inválido al crear el engagement.", ex);
        }

        await processHandler.HandleAsync(
            new ProcessEngagementCommand(engagementId),
            context.CancellationToken);

        logger.LogInformation("Engagement {EngagementId} procesado correctamente.", engagementId);
    }
}
