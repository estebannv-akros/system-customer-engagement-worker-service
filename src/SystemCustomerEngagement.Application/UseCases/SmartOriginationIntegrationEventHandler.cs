using AppMicroserviceCustomerEngagement.Application.Interfaces;

namespace AppMicroserviceCustomerEngagement.Application.UseCases;

public sealed class SmartOriginationIntegrationEventHandler(IHubSpotServiceProvider hubSpotServiceProvider)
{
    public async Task ExecuteAsync(
        IReadOnlyList<(string Email, string CurrentStep)> contacts,
        CancellationToken cancellationToken = default)
    {
        await hubSpotServiceProvider.UpsertContactsBatchAsync(contacts, cancellationToken);
    }
}
