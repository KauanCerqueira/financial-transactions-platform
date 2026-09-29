using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Application.Abstractions.Persistence;

public sealed record StatementFilter(TransactionType? Type, DateTimeOffset? From, DateTimeOffset? To)
{
    public static StatementFilter None { get; } = new(null, null, null);
}
