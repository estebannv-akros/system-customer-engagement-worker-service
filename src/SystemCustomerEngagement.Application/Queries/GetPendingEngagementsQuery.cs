using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Application.DTOs;

namespace SystemCustomerEngagement.Application.Queries;

public sealed record GetPendingEngagementsQuery(int BatchSize = 10) : IQuery<IEnumerable<EngagementDto>>;
