namespace app.microservice.customer.engagement.worker.Consumers.SmartOrigination;

public static class SmartOriginationIntegrationEventMapper
{
    public static IReadOnlyList<(string Email, string CurrentStep)> ToHubSpotContacts(
        IEnumerable<SmartOriginationIntegrationEvent> events,
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
