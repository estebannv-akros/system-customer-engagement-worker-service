using AppMicroserviceCustomerEngagement.Application.Interfaces;
using AppMicroserviceCustomerEngagement.Application.Models;
using AppMicroserviceCustomerEngagement.Domain.Enum;

namespace AppMicroserviceCustomerEngagement.Application.UseCases;

public sealed class SmartOriginationIntegrationEventHandler(IHubSpotServiceProvider hubSpotServiceProvider)
{
    public async Task ExecuteAsync(
        IReadOnlyList<HubSpotContact> contacts,
        CancellationToken cancellationToken = default)
    {
        await hubSpotServiceProvider.UpsertContactsBatchAsync(contacts, FlowPropertyEnum.SmartOrigination, cancellationToken);
    }
}
