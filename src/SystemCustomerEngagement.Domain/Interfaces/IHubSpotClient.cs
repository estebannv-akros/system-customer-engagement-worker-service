namespace SystemCustomerEngagement.Domain.Interfaces;

public interface IHubSpotClient
{
    Task UpsertContactsBatchAsync(
        IReadOnlyList<(string Email, string CurrentStep)> contacts,
        CancellationToken cancellationToken = default);
}
