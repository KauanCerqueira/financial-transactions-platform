namespace FinancialTransactions.Application.Dtos;

public sealed record StatementSummaryDto(
    int CreditCount,
    decimal CreditTotal,
    int DebitCount,
    decimal DebitTotal);
