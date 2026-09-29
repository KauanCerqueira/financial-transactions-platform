using FinancialTransactions.Application.Abstractions.Messaging;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed class EnqueueTransactionUseCase(
    ITransactionRepository transactions,
    ITransactionEventRepository transactionEvents,
    ITransactionQueue queue,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IEnqueueTransactionUseCase
{
    public async Task<EnqueuedTransaction> EnqueueAsync(
        ProcessTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var processed = await transactions.GetByEventIdAsync(command.EventId, cancellationToken);

        if (processed is not null)
        {
            return new EnqueuedTransaction(
                command.EventId,
                TransactionEventStatus.Processed,
                null,
                TransactionDto.From(processed));
        }

        var existingEvent = await transactionEvents.GetByEventIdAsync(command.EventId, cancellationToken);

        if (existingEvent is not null)
        {
            return new EnqueuedTransaction(
                command.EventId,
                existingEvent.Status,
                existingEvent.RejectionCode,
                null);
        }

        var amount = Money.Create(command.Amount);

        var transactionEvent = TransactionEvent.Receive(
            command.EventId,
            command.AccountId,
            command.Type,
            amount,
            command.OccurredAt,
            timeProvider.GetUtcNow());

        await transactionEvents.AddAsync(transactionEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await queue.PublishAsync(command, cancellationToken);

        return new EnqueuedTransaction(command.EventId, TransactionEventStatus.Pending, null, null);
    }
}
