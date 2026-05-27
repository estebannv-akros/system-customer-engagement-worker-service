using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SystemCustomerEngagement.Application.Interfaces;
using SystemCustomerEngagement.Domain.Exceptions;

namespace SystemCustomerEngagement.Infrastructure.HubSpot;

public sealed class HubSpotClient(
    HttpClient httpClient,
    ILogger<HubSpotClient> logger) : IHubSpotClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task UpsertContactsBatchAsync(
        IReadOnlyList<(string Email, string CurrentStep)> contacts,
        CancellationToken cancellationToken = default)
    {
        var inputs = contacts.Select(c => new UpsertInput(
            IdProperty: "email",
            Id: c.Email,
            Properties: new Dictionary<string, string>
            {
                ["email"]       = c.Email,
                ["paso_actual"] = c.CurrentStep
            }));

        var request = new BatchUpsertRequest(inputs);

        logger.LogDebug("HubSpot upsert batch Count={Count}", contacts.Count);

        var response = await httpClient.PostAsJsonAsync(
            "/crm/v3/objects/contacts/batch/upsert",
            request,
            JsonOptions,
            cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var statusCode = (int)response.StatusCode;

        logger.LogWarning(
            "HubSpot respondió {StatusCode} para batch de {Count} contactos. Body: {Body}",
            statusCode, contacts.Count, body);

        if (statusCode == 429 || statusCode >= 500)
            throw new TransientException($"HubSpot {statusCode} — reintentable. Body: {body}");

        throw new PermanentException($"HubSpot rechazó la solicitud {statusCode}. Body: {body}");
    }

    private sealed record BatchUpsertRequest(
        [property: JsonPropertyName("inputs")] IEnumerable<UpsertInput> Inputs);

    private sealed record UpsertInput(
        [property: JsonPropertyName("idProperty")] string IdProperty,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("properties")] Dictionary<string, string> Properties);
}
