using Microsoft.Extensions.DependencyInjection;
using SystemCustomerEngagement.Application.Commands;
using SystemCustomerEngagement.Application.Common;
using SystemCustomerEngagement.Application.DTOs;
using SystemCustomerEngagement.Application.Handlers;
using SystemCustomerEngagement.Application.Queries;

namespace SystemCustomerEngagement.Worker.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateEngagementCommand, Guid>, CreateEngagementCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessEngagementCommand>, ProcessEngagementCommandHandler>();
        services.AddScoped<IQueryHandler<GetPendingEngagementsQuery, IEnumerable<EngagementDto>>, GetPendingEngagementsQueryHandler>();
        return services;
    }
}
