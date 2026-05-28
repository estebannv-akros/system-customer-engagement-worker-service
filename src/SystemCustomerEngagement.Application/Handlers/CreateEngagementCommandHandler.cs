using SystemCustomerEngagement.Application.Commands;
using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Domain.Events;
using SystemCustomerEngagement.Domain.Entities;
using SystemCustomerEngagement.Domain.Repositories;
using SystemCustomerEngagement.Domain.ValueObjects;

namespace SystemCustomerEngagement.Application.Handlers;

public sealed class CreateEngagementCommandHandler(
    ICustomerEngagementRepository repository,
    IDomainEventDispatcher eventDispatcher)
    : ICommandHandler<CreateEngagementCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateEngagementCommand command, CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.Create(command.CustomerId);
        var engagement = CustomerEngagement.Create(customerId, command.Channel, command.Message);

        // TODO: descomentar cuando se integre persistencia
        // await repository.AddAsync(engagement, cancellationToken);

        await eventDispatcher.DispatchAsync(engagement.DomainEvents, cancellationToken);
        engagement.ClearDomainEvents();

        return engagement.Id;
    }
}
