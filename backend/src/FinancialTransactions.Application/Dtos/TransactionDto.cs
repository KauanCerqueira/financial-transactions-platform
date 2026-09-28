using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Application.Dtos;

public sealed record TransactionDto(
    Guid Id,
    Guid EventId,
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    DateTimeOffset OccurredAt,
    decimal BalanceAfter,
    DateTimeOffset RecordedAt)
{
    public static TransactionDto From(Transaction transaction) =>
        new(
            transaction.Id,
            transaction.EventId,
            transaction.AccountId,
            transaction.Type,
            transaction.Amount.Amount,
            transaction.OccurredAt,
            transaction.BalanceAfter.Amount,
            transaction.RecordedAt);
}
