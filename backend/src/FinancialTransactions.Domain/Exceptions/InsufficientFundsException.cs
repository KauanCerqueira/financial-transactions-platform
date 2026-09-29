using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.Domain.Exceptions;

public sealed class InsufficientFundsException : DomainException
{
    public Guid AccountId { get; }

    public Money Requested { get; }

    public Money Available { get; }

    public InsufficientFundsException(Guid accountId, Money requested, Money available)
        : base($"Saldo insuficiente na conta {accountId}. Solicitado: {requested}, disponível: {available}.")
    {
        AccountId = accountId;
        Requested = requested;
        Available = available;
    }
}
