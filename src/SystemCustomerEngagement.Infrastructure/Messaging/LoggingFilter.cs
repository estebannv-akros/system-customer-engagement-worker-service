using System.Diagnostics;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AppMicroserviceCustomerEngagement.Infrastructure.Messaging;

public sealed class LoggingFilter<T>(ILogger<LoggingFilter<T>> logger) : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var sw = Stopwatch.StartNew();

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["MessageId"]   = context.MessageId,
            ["CorrelationId"] = context.CorrelationId,
            ["MessageType"] = typeof(T).Name
        }))
        {
            try
            {
                await next.Send(context);
                logger.LogInformation("Mensaje procesado en {ElapsedMs}ms", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falló procesamiento tras {ElapsedMs}ms", sw.ElapsedMilliseconds);
                throw;
            }
        }
    }

    public void Probe(ProbeContext context) => context.CreateFilterScope("logging");
}
