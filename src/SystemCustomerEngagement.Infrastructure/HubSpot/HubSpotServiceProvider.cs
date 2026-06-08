using AppMicroserviceCustomerEngagement.Application.Interfaces;
using AppMicroserviceCustomerEngagement.Application.Models;
using AppMicroserviceCustomerEngagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AppMicroserviceCustomerEngagement.Infrastructure.HubSpot;

public sealed class HubSpotServiceProvider(
    HttpClient httpClient,
    ILogger<HubSpotServiceProvider> logger) : IHubSpotServiceProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task UpsertContactsBatchAsync(
        IReadOnlyList<HubSpotContact> contacts,
        string flow,
        CancellationToken cancellationToken = default)
    {
        var request = new {
            inputs = contacts.Select(i => new {
                id = i.CustomerId ?? i.Email,
                idProperty = i.CustomerId is null ? "email" : string.Empty,
                properties = new Dictionary<string, string>
                {
                    [flow] = i.CurrentStep,
                }
            })
        };

        var response = await httpClient.PostAsJsonAsync(
            "/crm/objects/2026-03/contacts/batch/update",
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
}
