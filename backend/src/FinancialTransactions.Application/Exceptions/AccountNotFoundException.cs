namespace FinancialTransactions.Application.Exceptions;

public sealed class AccountNotFoundException : Exception
{
    public AccountNotFoundException(Guid accountId)
        : base($"Conta {accountId} não encontrada.")
    {
    }
}
