namespace SystemCustomerEngagement.Application.Interfaces;

public interface IHubSpotClient
{
    Task UpsertContactAsync(
        string email,
        string CurrentStep,
        CancellationToken cancellationToken = default);
}
