using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Application.Abstractions.Persistence;

public sealed record StatementFilter(TransactionType? Type, DateTimeOffset? From, DateTimeOffset? To);
