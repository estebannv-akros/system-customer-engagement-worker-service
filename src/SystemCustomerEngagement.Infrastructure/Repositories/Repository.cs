using System.Collections.Concurrent;
using SystemCustomerEngagement.Domain.Entities;
using SystemCustomerEngagement.Domain.Repositories;

namespace SystemCustomerEngagement.Infrastructure.Repositories;

public sealed class Repository : IRepository
{
    private readonly ConcurrentDictionary<Guid, CustomerEngagement> _store = new();

    public Task AddAsync(CustomerEngagement engagement, CancellationToken cancellationToken = default)
    {
        _store[engagement.Id] = engagement;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CustomerEngagement>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var result = _store.Values
            .Where(e => e.Status == EngagementStatus.Pending)
            .Take(batchSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<CustomerEngagement>>(result);
    }

    public Task<CustomerEngagement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var engagement);
        return Task.FromResult(engagement);
    }

    public Task UpdateAsync(CustomerEngagement engagement, CancellationToken cancellationToken = default)
    {
        _store[engagement.Id] = engagement;
        return Task.CompletedTask;
    }
}
