using AppMicroserviceCustomerEngagement.Domain.Entities;

namespace AppMicroserviceCustomerEngagement.Domain.Repositories;

public interface IRepository
{
    Task AddAsync(CustomerEngagement engagement, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerEngagement>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<CustomerEngagement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerEngagement engagement, CancellationToken cancellationToken = default);
}
