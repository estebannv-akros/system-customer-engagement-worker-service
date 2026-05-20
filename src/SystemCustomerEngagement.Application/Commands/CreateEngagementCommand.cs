using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Domain.Entities;

namespace SystemCustomerEngagement.Application.Commands;

public sealed record CreateEngagementCommand(
    Guid CustomerId,
    EngagementChannel Channel,
    string Message) : ICommand<Guid>;
