using SystemCustomerEngagement.Application.Commands;
using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Application.Interfaces;
using SystemCustomerEngagement.Domain.Repositories;

namespace SystemCustomerEngagement.Application.Handlers;

public sealed class ProcessEngagementCommandHandler(
    ICustomerEngagementRepository repository,
    IDomainEventDispatcher eventDispatcher)
    : ICommandHandler<ProcessEngagementCommand>
{
    public async Task HandleAsync(ProcessEngagementCommand command, CancellationToken cancellationToken = default)
    {
        var engagement = await repository.GetByIdAsync(command.EngagementId, cancellationToken)
            ?? throw new InvalidOperationException($"Engagement {command.EngagementId} not found.");

        engagement.MarkAsProcessed();

        await repository.UpdateAsync(engagement, cancellationToken);
        await eventDispatcher.DispatchAsync(engagement.DomainEvents, cancellationToken);

        engagement.ClearDomainEvents();
    }
}
