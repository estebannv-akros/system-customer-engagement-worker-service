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
                $"No hay AccessToken de HubSpot configurado para BrandId {brandId} (CR=5, SV=7, GT=10, MX=12).");

        return token;
    }
}
