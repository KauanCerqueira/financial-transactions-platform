using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed record EnqueuedTransaction(
    Guid EventId,
    TransactionEventStatus Status,
    string? RejectionCode,
    TransactionDto? Transaction);
