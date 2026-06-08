using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppMicroserviceCustomerEngagement.Application.Interfaces;
using AppMicroserviceCustomerEngagement.Infrastructure.HubSpot;

namespace AppMicroserviceCustomerEngagement.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IHubSpotServiceProvider, HubSpotServiceProvider>(client =>
        {
            client.BaseAddress = new Uri(configuration["HubSpot:BaseUrl"]!);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", configuration["HubSpot:AccessToken"]!);
        });

        return services;
    }
}
