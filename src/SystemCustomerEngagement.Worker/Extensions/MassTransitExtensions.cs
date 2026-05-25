using MassTransit;
using SystemCustomerEngagement.Contracts.V1;
using SystemCustomerEngagement.Domain.Exceptions;
using SystemCustomerEngagement.Infrastructure.Messaging;
using SystemCustomerEngagement.Worker.Consumers;

namespace SystemCustomerEngagement.Worker.Extensions;

public static class MassTransitExtensions
{
    private const string QueueName = "customer-engagement.engagements";

    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CustomerEngagementConsumer>();

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

                cfg.ReceiveEndpoint(QueueName, e =>
                {
                    // Quorum queue: replicada, durable, tolerante a fallos
                    e.SetQuorumQueue();

                    // Concurrencia según lineamientos: PrefetchCount ≈ ConcurrentMessageLimit × 2
                    e.PrefetchCount = 16;
                    e.ConcurrentMessageLimit = 8;

                    // Logging estructurado en cada mensaje (MessageId, CorrelationId, MessageType)
                    e.UseConsumeFilter(typeof(LoggingFilter<>), context);

                    // Reintentos rápidos en memoria: deadlocks, timeouts cortos
                    e.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retryLimit: 3,
                            minInterval: TimeSpan.FromMilliseconds(100),
                            maxInterval: TimeSpan.FromSeconds(2),
                            intervalDelta: TimeSpan.FromMilliseconds(200));

                        // Errores permanentes van directo a DLQ sin reintentar
                        r.Ignore<PermanentException>();
                    });

                    // Reentregas con delay: rate limit, dependencias caídas
                    // Requiere plugin rabbitmq_delayed_message_exchange
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
