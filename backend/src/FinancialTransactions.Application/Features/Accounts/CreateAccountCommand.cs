namespace FinancialTransactions.Application.Features.Accounts;

public sealed record CreateAccountCommand(string HolderName, decimal InitialBalance);
