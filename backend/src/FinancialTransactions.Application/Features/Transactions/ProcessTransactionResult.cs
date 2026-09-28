using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed record ProcessTransactionResult(TransactionDto Transaction, bool AlreadyProcessed);
