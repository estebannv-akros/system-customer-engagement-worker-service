using System.Collections.Concurrent;
using SystemCustomerEngagement.Domain.Entities;
using SystemCustomerEngagement.Domain.Repositories;

namespace SystemCustomerEngagement.Infrastructure.Repositories;

public sealed class InMemoryCustomerEngagementRepository : ICustomerEngagementRepository
{
    private readonly ConcurrentDictionary<Guid, CustomerEngagement> _store = new();

    public Task<CustomerEngagement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task<IEnumerable<CustomerEngagement>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var result = _store.Values
            .Where(e => e.Status == EngagementStatus.Pending)
            .Take(batchSize);

        return Task.FromResult(result);
    }

    public Task AddAsync(CustomerEngagement engagement, CancellationToken cancellationToken = default)
    {
        _store[engagement.Id] = engagement;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CustomerEngagement engagement, CancellationToken cancellationToken = default)
    {
        _store[engagement.Id] = engagement;
        return Task.CompletedTask;
    }
}
