using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.Domain.Exceptions;

/// <summary>
/// Lançada quando uma conta tenta debitar mais do que possui.
/// Carrega os números envolvidos para que a API possa devolver uma mensagem clara.
/// </summary>
public sealed class InsufficientFundsException : DomainException
{
    public Guid AccountId { get; }
    public Money Requested { get; }
    public Money Available { get; }

    public InsufficientFundsException(Guid accountId, Money requested, Money available)
        : base($"Saldo insuficiente na conta {accountId}. " +
               $"Solicitado: {requested}, disponível: {available}.")
    {
        AccountId = accountId;
        Requested = requested;
        Available = available;
    }
}
