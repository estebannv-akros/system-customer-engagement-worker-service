using SystemCustomerEngagement.Application.Commands;
using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Domain.Events;
using SystemCustomerEngagement.Domain.Repositories;

namespace SystemCustomerEngagement.Application.Handlers;

public sealed class ProcessEngagementCommandHandler(
    ICustomerEngagementRepository repository,
    IDomainEventDispatcher eventDispatcher)
    : ICommandHandler<ProcessEngagementCommand>
{
    public async Task HandleAsync(ProcessEngagementCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: descomentar cuando se integre persistencia
        // var engagement = await repository.GetByIdAsync(command.EngagementId, cancellationToken)
        //     ?? throw new InvalidOperationException($"Engagement {command.EngagementId} not found.");
        // engagement.MarkAsProcessed();
        // await repository.UpdateAsync(engagement, cancellationToken);
        // await eventDispatcher.DispatchAsync(engagement.DomainEvents, cancellationToken);
        // engagement.ClearDomainEvents();

        await Task.CompletedTask;
    }
}
