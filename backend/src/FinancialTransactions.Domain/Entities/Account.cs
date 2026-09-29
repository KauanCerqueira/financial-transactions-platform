using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.Exceptions;
using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.Domain.Entities;

public sealed class Account
{
    private readonly List<Transaction> _transactions = [];

    public Guid Id { get; private set; }

    public string HolderName { get; private set; } = string.Empty;

    public Money Balance { get; private set; } = Money.Zero;

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() // EF Core
    {
    }

    public static Account Create(Guid id, string holderName, Money initialBalance, DateTimeOffset now)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("O identificador da conta é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(holderName))
        {
            throw new DomainException("O nome do titular é obrigatório.");
        }

        return new Account
        {
            Id = id,
            HolderName = holderName.Trim(),
            Balance = initialBalance,
            CreatedAt = now
        };
    }

    public Transaction RegisterTransaction(
        Guid eventId,
        TransactionType type,
        Money amount,
        DateTimeOffset occurredAt,
        DateTimeOffset recordedAt)
    {
        if (eventId == Guid.Empty)
        {
            throw new DomainException("O eventId do evento é obrigatório.");
        }

        if (amount.IsZero)
        {
            throw new DomainException("O valor do evento deve ser maior que zero.");
        }

        if (occurredAt == default)
        {
            throw new DomainException("A data do evento é obrigatória.");
        }

        Balance = type switch
        {
            TransactionType.Credit => Balance.Add(amount),
            TransactionType.Debit when Balance >= amount => Balance.Subtract(amount),
            TransactionType.Debit => throw new InsufficientFundsException(Id, amount, Balance),
            _ => throw new DomainException($"Tipo de transação inválido: {type}.")
        };

        var transaction = new Transaction(
            Guid.NewGuid(),
            eventId,
            Id,
            type,
            amount,
            occurredAt,
            Balance,
            recordedAt);

        _transactions.Add(transaction);
        return transaction;
    }
}
