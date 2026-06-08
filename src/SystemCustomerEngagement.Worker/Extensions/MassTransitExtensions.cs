using app.microservice.customer.engagement.worker.Consumers;
using app.microservice.customer.engagement.worker.Contracts;
using AppMicroserviceCustomerEngagement.Domain.Exceptions;
using AppMicroserviceCustomerEngagement.Infrastructure.Messaging;
using MassTransit;

namespace AppMicroserviceCustomerEngagement.Worker.Extensions;

public static class MassTransitExtensions
{
    private const string CreditOriginationQueue =
        "customer_engagement_upsert_credit_origination_integration_event";

    private const string SmartOriginationQueue =
        "customer_engagement_upsert_smart_origination_integration_event";

    private const string UserOriginationQueue =
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

                cfg.ReceiveEndpoint(CreditOriginationQueue, e =>
                {
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

                    e.Batch<CreditOriginationIntegrationEvent>(b =>
                    {
                        b.MessageLimit = 20;
                        b.TimeLimit = TimeSpan.FromSeconds(5);
                    });

                    e.ConfigureConsumer<CreditOriginationConsumer>(context);
                });

                cfg.ReceiveEndpoint(SmartOriginationQueue, e =>
                {
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

                    e.Batch<SmartOriginationIntegrationEvent>(b =>
                    {
                        b.MessageLimit = 20;
                        b.TimeLimit = TimeSpan.FromSeconds(5);
                    });

                    e.ConfigureConsumer<SmartOriginationConsumer>(context);
                });

                cfg.ReceiveEndpoint(UserOriginationQueue, e =>
                {
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

                    e.Batch<UserOriginationConsumer>(b =>
                    {
                        b.MessageLimit = 20;
                        b.TimeLimit = TimeSpan.FromSeconds(5);
                    });

                    e.ConfigureConsumer<UserOriginationConsumer>(context);
                });
            });
        });

        return services;
    }
}
