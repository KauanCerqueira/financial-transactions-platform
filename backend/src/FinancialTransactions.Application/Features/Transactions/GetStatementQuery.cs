using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed record GetStatementQuery(
    Guid AccountId,
    int Page,
    int PageSize,
    TransactionType? Type = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);
