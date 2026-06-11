using AppMicroserviceCustomerEngagement.Domain.Constants;
using MassTransit;
using app.microservice.customer.engagement.worker.Contracts;

namespace SystemCustomerEngagement.Contracts;

public static class HubSpotPublishExtensions
{
    public static Task PublishCreditOriginationAsync(
        this IPublishEndpoint publishEndpoint,
        CreditOriginationIntegrationEvent message,
        CancellationToken cancellationToken = default) =>
        publishEndpoint.PublishByBrandId(message, message.BrandId, cancellationToken);

    public static Task PublishSmartOriginationAsync(
        this IPublishEndpoint publishEndpoint,
        SmartOriginationIntegrationEvent message,
        CancellationToken cancellationToken = default) =>
        publishEndpoint.PublishByBrandId(message, message.BrandId, cancellationToken);

    public static Task PublishUserRegistrationAsync(
        this IPublishEndpoint publishEndpoint,
        UserOriginationIntegrationEvent message,
        CancellationToken cancellationToken = default) =>
        publishEndpoint.PublishByBrandId(message, message.BrandId, cancellationToken);

    public static Task PublishByBrandId<T>(
        this IPublishEndpoint publishEndpoint,
        T message,
        int brandId,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var routingKey = HubSpotCountries.GetRoutingKey(brandId);

        return publishEndpoint.Publish(
            message,
            context => context.SetRoutingKey(routingKey),
            cancellationToken);
    }
}
