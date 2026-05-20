using SystemCustomerEngagement.Application.Common;

namespace SystemCustomerEngagement.Application.Commands;

public sealed record ProcessEngagementCommand(Guid EngagementId) : ICommand;
