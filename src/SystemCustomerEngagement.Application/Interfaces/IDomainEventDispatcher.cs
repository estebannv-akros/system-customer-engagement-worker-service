using AppMicroserviceCustomerEngagement.Domain.Common;

namespace AppMicroserviceCustomerEngagement.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
