using SystemCustomerEngagement.Application.Commands;
using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Application.Interfaces;
using SystemCustomerEngagement.Domain.Repositories;

namespace SystemCustomerEngagement.Application.Handlers;

public sealed class CreateEngagementCommandHandler(
    IRepository repository,
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
