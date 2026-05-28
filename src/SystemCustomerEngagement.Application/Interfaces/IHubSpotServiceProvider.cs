namespace AppMicroserviceCustomerEngagement.Application.Interfaces;

public interface IHubSpotServiceProvider
{
    Task UpsertContactsBatchAsync(
        IReadOnlyList<(string Email, string CurrentStep)> contacts,
        CancellationToken cancellationToken = default);
}
