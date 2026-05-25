namespace SystemCustomerEngagement.Domain.Exceptions;

/// <summary>
/// Señala un error reintentable: deadlocks, timeouts cortos, rate limits, dependencias temporalmente caídas.
/// MassTransit ejecutará retry/redelivery antes de mover a DLQ.
/// </summary>
public sealed class TransientException : Exception
{
    public TransientException(string message, Exception? inner = null)
        : base(message, inner) { }
}
