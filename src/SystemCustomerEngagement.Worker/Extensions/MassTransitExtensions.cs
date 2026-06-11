using app.microservice.customer.engagement.worker.Consumers;
using AppMicroserviceCustomerEngagement.Domain.Constants;
using AppMicroserviceCustomerEngagement.Domain.Exceptions;
using AppMicroserviceCustomerEngagement.Infrastructure.Messaging;
using MassTransit;
using RabbitMQ.Client;
using app.microservice.customer.engagement.worker.Contracts;

namespace AppMicroserviceCustomerEngagement.Worker.Extensions;

public static class MassTransitExtensions
{
    public const string CreditOriginationQueueBase =
        "customer_engagement_upsert_credit_origination_integration_event";

    public const string SmartOriginationQueueBase =
        "customer_engagement_upsert_smart_origination_integration_event";

    public const string UserOriginationQueueBase =
        "customer_engagement_upsert_user_registration_integration_event";

    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CreditOriginationConsumer>(cfg =>
            {
                cfg.Options<BatchOptions>(b =>
                {
                    b.MessageLimit = 10;
                    b.TimeLimit = TimeSpan.FromSeconds(30);
                    b.ConcurrencyLimit = 4;
                });
            });

            x.AddConsumer<SmartOriginationConsumer>(cfg =>
            {
                cfg.Options<BatchOptions>(b =>
                {
                    b.MessageLimit = 10;
                    b.TimeLimit = TimeSpan.FromSeconds(30);
                    b.ConcurrencyLimit = 4;
                });
            });

            x.AddConsumer<UserOriginationConsumer>(cfg =>
            {
                cfg.Options<BatchOptions>(b =>
                {
                    b.MessageLimit = 10;
                    b.TimeLimit = TimeSpan.FromSeconds(30);
                    b.ConcurrencyLimit = 4;
                });
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:Host"]!;
                var port = configuration.GetValue<ushort>("RabbitMq:Port", 5671);
                var vhost = configuration["RabbitMq:VirtualHost"] ?? "/";
                var useSsl = configuration.GetValue<bool>("RabbitMq:UseSsl", true);

                cfg.Host(host, port, vhost, h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);

                    if (useSsl)
                        h.UseSsl(s => s.ServerName = host);
                });

                ConfigurePublishTopology<CreditOriginationIntegrationEvent>(cfg);
                ConfigurePublishTopology<SmartOriginationIntegrationEvent>(cfg);
                ConfigurePublishTopology<UserOriginationIntegrationEvent>(cfg);

                foreach (var country in HubSpotCountries.All)
                {
                    ConfigureCountryEndpoint<CreditOriginationIntegrationEvent, CreditOriginationConsumer>(
                        cfg, context, CreditOriginationQueueBase, country.Code);

                    ConfigureCountryEndpoint<SmartOriginationIntegrationEvent, SmartOriginationConsumer>(
                        cfg, context, SmartOriginationQueueBase, country.Code);

                    ConfigureCountryEndpoint<UserOriginationIntegrationEvent, UserOriginationConsumer>(
                        cfg, context, UserOriginationQueueBase, country.Code);
                }
            });
        });

        return services;
    }

    private static void ConfigurePublishTopology<TMessage>(IRabbitMqBusFactoryConfigurator cfg)
        where TMessage : class
    {
        cfg.Publish<TMessage>(publisher => publisher.ExchangeType = ExchangeType.Topic);
    }

    private static void ConfigureCountryEndpoint<TMessage, TConsumer>(
        IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        string baseQueue,
        string routingKey)
        where TMessage : class
        where TConsumer : class, IConsumer
    {
        cfg.ReceiveEndpoint($"{baseQueue}_{routingKey}", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Bind<TMessage>(bind => bind.RoutingKey = routingKey);

            e.SetQuorumQueue();
            e.PrefetchCount = 40;
            e.UseConsumeFilter(typeof(LoggingFilter<>), context);

            e.UseMessageRetry(r =>
            {
                r.Exponential(
                    retryLimit: 3,
                    minInterval: TimeSpan.FromMilliseconds(100),
                    maxInterval: TimeSpan.FromSeconds(2),
                    intervalDelta: TimeSpan.FromMilliseconds(200));

                r.Ignore<PermanentException>();
            });

            e.UseDelayedRedelivery(r => r.Intervals(
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(2),
                TimeSpan.FromMinutes(10)));

            e.Batch<TMessage>(b =>
            {
                b.MessageLimit = 20;
                b.TimeLimit = TimeSpan.FromSeconds(5);
            });

            e.ConfigureConsumer<TConsumer>(context);
        });
    }
}
