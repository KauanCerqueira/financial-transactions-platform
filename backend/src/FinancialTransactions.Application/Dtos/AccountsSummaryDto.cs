namespace FinancialTransactions.Application.Dtos;

public sealed record AccountsSummaryDto(int Accounts, decimal TotalBalance, int Transactions);
