namespace FinancialTransactions.Application.Exceptions;

public sealed class AccountNotFoundException : NotFoundException
{
    public AccountNotFoundException(Guid accountId)
        : base($"Conta {accountId} não encontrada.")
    {
    }
}
