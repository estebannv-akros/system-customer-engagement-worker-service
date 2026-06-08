using AppMicroserviceCustomerEngagement.Application.Models;

namespace AppMicroserviceCustomerEngagement.Application.Interfaces;

public interface IHubSpotServiceProvider
{
    Task UpsertContactsBatchAsync(
        IReadOnlyList<HubSpotContact> contacts,
        string flow,
        CancellationToken cancellationToken = default);
}
