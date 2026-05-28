using AppMicroserviceCustomerEngagement.Application.UseCases;

namespace AppMicroserviceCustomerEngagement.Worker.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreditOriginationIntegrationEventHandler>();
        services.AddScoped<SmartOriginationIntegrationEventHandler>();
        services.AddScoped<UpdateUserIntegrationEventHandler>();
        return services;
    }
}
