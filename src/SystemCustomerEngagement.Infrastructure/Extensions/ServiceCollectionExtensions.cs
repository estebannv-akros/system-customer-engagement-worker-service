using Microsoft.Extensions.DependencyInjection;
using SystemCustomerEngagement.Application.Interfaces;
using SystemCustomerEngagement.Domain.Repositories;
using SystemCustomerEngagement.Infrastructure.Messaging;
using SystemCustomerEngagement.Infrastructure.Repositories;

namespace SystemCustomerEngagement.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ICustomerEngagementRepository, InMemoryCustomerEngagementRepository>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
}
