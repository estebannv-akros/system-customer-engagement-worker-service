using AppMicroserviceCustomerEngagement.Domain.Constants;
using AppMicroserviceCustomerEngagement.Application.Interfaces;
using AppMicroserviceCustomerEngagement.Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace AppMicroserviceCustomerEngagement.Infrastructure.HubSpot;

public sealed class HubSpotAccessTokenProvider(IConfiguration configuration) : IHubSpotAccessTokenProvider
{
    public string GetAccessToken(int brandId)
    {
        var token = configuration[$"HubSpot:AccessTokens:{brandId}"];

        if (string.IsNullOrWhiteSpace(token))
            throw new PermanentException(
                $"No hay AccessToken de HubSpot configurado para BrandId {brandId}. Países: {string.Join(", ", HubSpotCountries.All.Select(c => $"{c.Code.ToUpperInvariant()}={c.BrandId}"))}.");

        return token;
    }
}
