namespace AppMicroserviceCustomerEngagement.Domain.Constants;

public static class HubSpotCountries
{
    public static readonly HubSpotCountry[] All =
    [
        new((int)Country.CR, Country.CR.ToString().ToLower()),
        new((int)Country.SV, Country.SV.ToString().ToLower()),
        new((int)Country.GT, Country.GT.ToString().ToLower()),
        new((int)Country.MX, Country.MX.ToString().ToLower()),
    ];

    public static bool TryGetRoutingKey(int brandId, out string routingKey)
    {
        foreach (var country in All)
        {
            if (country.BrandId != brandId)
                continue;

            routingKey = country.Code;
            return true;
        }

        routingKey = string.Empty;
        return false;
    }

    private enum Country
    {
        CR = 5,
        SV = 7,
        GT = 10,
        MX = 12,
    }

    public static string GetRoutingKey(int brandId) =>
        TryGetRoutingKey(brandId, out var routingKey)
            ? routingKey
            : throw new ArgumentOutOfRangeException(
                nameof(brandId),
                brandId,
                "BrandId no soportado");
}

public readonly record struct HubSpotCountry(int BrandId, string Code);
