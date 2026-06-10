using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AppMicroserviceCustomerEngagement.Application.Interfaces;
using AppMicroserviceCustomerEngagement.Application.Models;
using AppMicroserviceCustomerEngagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AppMicroserviceCustomerEngagement.Infrastructure.HubSpot;

public sealed class HubSpotServiceProvider(
    HttpClient httpClient,
    IHubSpotAccessTokenProvider accessTokenProvider,
    ILogger<HubSpotServiceProvider> logger) : IHubSpotServiceProvider
{
    public async Task UpsertContactsBatchAsync(
        IReadOnlyList<HubSpotContact> contacts,
        string flow,
        CancellationToken cancellationToken = default)
    {
        foreach (var group in contacts.GroupBy(c => c.BrandId))
        {
            await UpsertBrandBatchAsync(group.ToList(), group.Key, flow, cancellationToken);
        }
    }

    private async Task UpsertBrandBatchAsync(
        IReadOnlyList<HubSpotContact> contacts,
        int brandId,
        string flow,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            inputs = contacts.Select(i => new
            {
                id = i.Email,
                idProperty = "email",
                properties = new Dictionary<string, string>
                {
                    [flow] = i.Message,
                }
            })
        };

        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "/crm/objects/2026-03/contacts/batch/update");

        requestMessage.Content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessTokenProvider.GetAccessToken(brandId));

        var response = await httpClient.SendAsync(requestMessage, cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var statusCode = (int)response.StatusCode;

        logger.LogWarning(
            "HubSpot respondió {StatusCode} para batch de {Count} contactos (BrandId={BrandId}). Body: {Body}",
            statusCode, contacts.Count, brandId, body);

        if (statusCode == 429 || statusCode >= 500)
            throw new TransientException(
                $"HubSpot {statusCode} — reintentable (BrandId={brandId}). Body: {body}");

        throw new PermanentException(
            $"HubSpot rechazó la solicitud {statusCode} (BrandId={brandId}). Body: {body}");
    }
}
