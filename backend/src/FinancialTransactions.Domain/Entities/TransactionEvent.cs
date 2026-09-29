using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.Exceptions;
using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.Domain.Entities;

public sealed class TransactionEvent
{
    public Guid EventId { get; private set; }

    public Guid AccountId { get; private set; }

    public TransactionType Type { get; private set; }

    public Money Amount { get; private set; } = Money.Zero;

    public DateTimeOffset OccurredAt { get; private set; }

    public TransactionEventStatus Status { get; private set; }

    public string? RejectionCode { get; private set; }

    public DateTimeOffset ReceivedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    private TransactionEvent() // EF Core
    {
    }

    public static TransactionEvent Receive(
        Guid eventId,
        Guid accountId,
        TransactionType type,
        Money amount,
        DateTimeOffset occurredAt,
        DateTimeOffset receivedAt)
    {
        if (eventId == Guid.Empty)
        {
            throw new DomainException("O eventId do evento é obrigatório.");
        }

        if (accountId == Guid.Empty)
        {
            throw new DomainException("A conta do evento é obrigatória.");
        }

        if (amount.IsZero)
        {
            throw new DomainException("O valor do evento deve ser maior que zero.");
        }

        if (occurredAt == default)
        {
            throw new DomainException("A data do evento é obrigatória.");
        }

        return new TransactionEvent
        {
            EventId = eventId,
            AccountId = accountId,
            Type = type,
            Amount = amount,
            OccurredAt = occurredAt,
            Status = TransactionEventStatus.Pending,
            ReceivedAt = receivedAt
        };
    }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        Status = TransactionEventStatus.Processed;
        ProcessedAt = processedAt;
        RejectionCode = null;
    }

    public void MarkRejected(string rejectionCode, DateTimeOffset processedAt)
    {
        Status = TransactionEventStatus.Rejected;
        ProcessedAt = processedAt;
        RejectionCode = rejectionCode;
    }
}
