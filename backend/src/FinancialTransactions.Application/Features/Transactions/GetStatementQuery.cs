namespace FinancialTransactions.Application.Features.Transactions;

public sealed record GetStatementQuery(Guid AccountId, int Page, int PageSize);
