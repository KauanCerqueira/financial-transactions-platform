using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed record ProcessTransactionCommand(
    Guid EventId,
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    DateTimeOffset OccurredAt);
