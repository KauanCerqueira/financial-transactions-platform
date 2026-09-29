using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Api.Contracts;

public sealed record TransactionAcceptedResponse(
    Guid EventId,
    TransactionEventStatus Status,
    string? RejectionCode,
    TransactionDto? Transaction);
