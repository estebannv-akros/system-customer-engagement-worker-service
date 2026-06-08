namespace app.microservice.customer.engagement.worker.Contracts.UserOrigination;

public static class UserOriginationIntegrationEventMapper
{
    public static IReadOnlyList<(string Email, string CurrentStep)> ToHubSpotContacts(
        IEnumerable<UserOriginationIntegrationEvent> events,
        ILogger logger)
    {
        var result = new List<(string, string)>();

        foreach (var msg in events)
        {
            if (string.IsNullOrWhiteSpace(msg.Email) || string.IsNullOrWhiteSpace(msg.CurrentStep))
            {
                logger.LogWarning(
                    "Mensaje descartado — Email o CurrentStep vacío. CustomerId={CustomerId}",
                    msg.CustomerId);
                continue;
            }

            result.Add((msg.Email, msg.CurrentStep));
        }

        return result;
    }
}
