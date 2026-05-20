using SystemCustomerEngagement.Domain.Entities;
using SystemCustomerEngagement.Domain.ValueObjects;

namespace SystemCustomerEngagement.Domain.Repositories;

public interface ICustomerEngagementRepository
{
    Task<CustomerEngagement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerEngagement>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task AddAsync(CustomerEngagement engagement, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerEngagement engagement, CancellationToken cancellationToken = default);
}
