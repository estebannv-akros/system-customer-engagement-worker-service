using app.microservice.customer.engagement.worker.Contracts;

namespace app.microservice.customer.engagement.worker.Contracts.CreditOrigination;

public static class CreditOriginationIntegrationEventMapper
{
    public static IReadOnlyList<(string Email, string CurrentStep)> ToHubSpotContacts(
        IEnumerable<CreditOriginationIntegrationEvent> events,
        ILogger logger)
    {
        var result = new List<(string, string)>();

        foreach (var msg in events)
        {

            if (string.IsNullOrWhiteSpace(msg.Email) || string.IsNullOrWhiteSpace(msg.CurrentStep))
            {
                logger.LogWarning(
                    "Mensaje descartado — Email o CurrentStep vacío. CustomerId={CustomerId}",
                    msg.ContactId);
                continue;
            }

            result.Add((msg.Email, msg.CurrentStep));
        }

        return result;
    }
}
