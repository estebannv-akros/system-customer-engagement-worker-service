namespace AppMicroserviceCustomerEngagement.Domain.Exceptions;

/// <summary>
/// Señala un error no reintentable: payload inválido, dato corrupto, rechazo 4xx.
/// MassTransit enviará el mensaje directo a DLQ sin reintentar.
/// </summary>
public sealed class PermanentException : Exception
{
    public PermanentException(string message, Exception? inner = null)
        : base(message, inner) { }
}
