namespace SystemCustomerEngagement.Application.Interfaces;

public interface IHubSpotClient
{
    Task UpsertContactAsync(
        string email,
        string pasoActual,
        CancellationToken cancellationToken = default);
}
