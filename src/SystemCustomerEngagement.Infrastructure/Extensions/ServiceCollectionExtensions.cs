using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemCustomerEngagement.Application.Interfaces;
using SystemCustomerEngagement.Domain.Repositories;
using SystemCustomerEngagement.Infrastructure.HubSpot;
using SystemCustomerEngagement.Infrastructure.Messaging;
using SystemCustomerEngagement.Infrastructure.Repositories;

namespace SystemCustomerEngagement.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: reemplazar por repositorio real cuando se integre persistencia
        services.AddSingleton<ICustomerEngagementRepository, InMemoryCustomerEngagementRepository>();

        // Scoped porque depende de IPublishEndpoint que es scoped (contexto del consumer activo)
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddHttpClient<IHubSpotClient, HubSpotClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["HubSpot:BaseUrl"]!);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", configuration["HubSpot:AccessToken"]!);
        });

        return services;
    }
}
