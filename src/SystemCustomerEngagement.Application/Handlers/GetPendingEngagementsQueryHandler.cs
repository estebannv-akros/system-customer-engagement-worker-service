using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Application.DTOs;
using SystemCustomerEngagement.Application.Queries;
using SystemCustomerEngagement.Domain.Repositories;

namespace SystemCustomerEngagement.Application.Handlers;

public sealed class GetPendingEngagementsQueryHandler(IRepository repository)
    : IQueryHandler<GetPendingEngagementsQuery, IEnumerable<EngagementDto>>
{
    public async Task<IEnumerable<EngagementDto>> HandleAsync(GetPendingEngagementsQuery query, CancellationToken cancellationToken = default)
    {
        var engagements = await repository.GetPendingAsync(query.BatchSize, cancellationToken);

        return engagements.Select(e => new EngagementDto(
            e.Id,
            e.CustomerId,
            e.Channel,
            e.Status,
            e.Message,
            e.CreatedAt,
            e.ProcessedAt));
    }
}
