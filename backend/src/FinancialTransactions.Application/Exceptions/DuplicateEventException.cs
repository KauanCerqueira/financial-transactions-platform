namespace FinancialTransactions.Application.Exceptions;

public sealed class DuplicateEventException : Exception
{
    public Guid EventId { get; }

    public DuplicateEventException(Guid eventId)
        : base($"O evento {eventId} já foi processado.")
    {
        EventId = eventId;
    }
}
