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

    public async Task UpsertContactAsync(
        string email,
        string pasoActual,
        CancellationToken cancellationToken = default)
    {
        var request = new BatchUpsertRequest(
        [
            new UpsertInput(
                IdProperty: "email",
                Id: email,
                Properties: new Dictionary<string, string>
                {
                    ["email"]       = email,
                    ["paso_actual"] = pasoActual
                })
        ]);

        logger.LogDebug(
            "HubSpot upsert contact Email={Email} PasoActual={PasoActual}",
            email, pasoActual);

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
            "HubSpot respondió {StatusCode} para Email={Email}. Body: {Body}",
            statusCode, email, body);

        if (statusCode == 429 || statusCode >= 500)
            throw new TransientException($"HubSpot {statusCode} — reintentable. Body: {body}");

        throw new PermanentException($"HubSpot rechazó la solicitud {statusCode}. Body: {body}");
    }

    private sealed record BatchUpsertRequest(
        [property: JsonPropertyName("inputs")] IReadOnlyList<UpsertInput> Inputs);

    private sealed record UpsertInput(
        [property: JsonPropertyName("idProperty")] string IdProperty,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("properties")] Dictionary<string, string> Properties);
}
