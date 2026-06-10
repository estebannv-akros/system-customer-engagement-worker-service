namespace AppMicroserviceCustomerEngagement.Application.Interfaces;

public interface IHubSpotAccessTokenProvider
{
    string GetAccessToken(int brandId);
}
