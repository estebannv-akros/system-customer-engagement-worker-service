using MassTransit;
using SystemCustomerEngagement.Contracts.V1;
using SystemCustomerEngagement.Domain.Exceptions;
using SystemCustomerEngagement.Infrastructure.Messaging;
using SystemCustomerEngagement.Worker.Consumers;

namespace SystemCustomerEngagement.Worker.Extensions;

public static class MassTransitExtensions
{
    private const string EngagementsQueue   = "customer-engagement.engagements";
    private const string NotificationsQueue = "customer-engagement.notifications";
    private const string InteractionsQueue  = "customer-engagement.interactions";

    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CustomerEngagementConsumer>(cfg =>
            {
                cfg.Options<BatchOptions>(b =>
                {
                    b.MessageLimit = 10;
                    b.TimeLimit    = TimeSpan.FromSeconds(30);
                    b.ConcurrencyLimit = 4;
                });
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var host     = configuration["RabbitMq:Host"]!;
                var port     = configuration.GetValue<ushort>("RabbitMq:Port", 5671);
                var vhost    = configuration["RabbitMq:VirtualHost"] ?? "/";
                var useSsl   = configuration.GetValue<bool>("RabbitMq:UseSsl", true);

                cfg.Host(host, port, vhost, h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);

                    if (useSsl)
                        h.UseSsl(s => s.ServerName = host);
                });

                // Cola: engagements
                cfg.ReceiveEndpoint(EngagementsQueue, e =>
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

                    e.ConfigureConsumer<CustomerEngagementConsumer>(context);
                });

                // Cola: notifications
                cfg.ReceiveEndpoint(NotificationsQueue, e =>
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

                    e.ConfigureConsumer<CustomerEngagementConsumer>(context);
                });

                // Cola: interactions
                cfg.ReceiveEndpoint(InteractionsQueue, e =>
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

                    e.ConfigureConsumer<CustomerEngagementConsumer>(context);
                });
            });
        });

        return services;
    }
}
