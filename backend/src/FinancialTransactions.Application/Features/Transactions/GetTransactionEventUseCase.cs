using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed class GetTransactionEventUseCase(
    ITransactionEventRepository transactionEvents,
    ITransactionRepository transactions) : IGetTransactionEventUseCase
{
    public async Task<EnqueuedTransaction?> GetAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var transactionEvent = await transactionEvents.GetByEventIdAsync(eventId, cancellationToken);

        if (transactionEvent is null)
        {
            return null;
        }

        var transaction = transactionEvent.Status == TransactionEventStatus.Processed
            ? await GetProcessedTransactionAsync(eventId, cancellationToken)
            : null;

        return new EnqueuedTransaction(
            eventId,
            transactionEvent.Status,
            transactionEvent.RejectionCode,
            transaction);
    }

    private async Task<TransactionDto?> GetProcessedTransactionAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var processed = await transactions.GetByEventIdAsync(eventId, cancellationToken);

        return processed is null ? null : TransactionDto.From(processed);
    }
}
