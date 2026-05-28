using Microsoft.Extensions.Logging;
using AppMicroserviceCustomerEngagement.Domain.Entities;
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
            if (!Enum.TryParse<EngagementChannel>(msg.Channel, ignoreCase: true, out _))
            {
                logger.LogWarning(
                    "Mensaje descartado — canal desconocido: '{Channel}'. CustomerId={CustomerId}",
                    msg.Channel, msg.CustomerId);
                continue;
            }

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
