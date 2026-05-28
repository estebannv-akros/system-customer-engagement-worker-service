using AppMicroserviceCustomerEngagement.Worker.Contracts;

namespace AppMicroserviceCustomerEngagement.Worker.Consumers;

public static class CreditOriginationIntegrationEventMapper
{
    public static IReadOnlyList<(string Email, string CurrentStep)> ToHubSpotContacts(
        IEnumerable<CreditFlowStepIntegrationEvent> events,
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
