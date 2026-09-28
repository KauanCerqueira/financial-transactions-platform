using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.Exceptions;
using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid AccountId { get; private set; }

    public TransactionType Type { get; private set; }

    public Money Amount { get; private set; } = Money.Zero;

    public DateTimeOffset OccurredAt { get; private set; }

    public Money BalanceAfter { get; private set; } = Money.Zero;

    public DateTimeOffset RecordedAt { get; private set; }

    private Transaction() // EF Core
    {
    }

    internal Transaction(
        Guid id,
        Guid eventId,
        Guid accountId,
        TransactionType type,
        Money amount,
        DateTimeOffset occurredAt,
        Money balanceAfter,
        DateTimeOffset recordedAt)
    {
        if (eventId == Guid.Empty)
        {
            throw new DomainException("O eventId do lançamento é obrigatório.");
        }

        if (amount.IsZero)
        {
            throw new DomainException("O valor do lançamento deve ser maior que zero.");
        }

        Id = id;
        EventId = eventId;
        AccountId = accountId;
        Type = type;
        Amount = amount;
        OccurredAt = occurredAt;
        BalanceAfter = balanceAfter;
        RecordedAt = recordedAt;
    }
}
