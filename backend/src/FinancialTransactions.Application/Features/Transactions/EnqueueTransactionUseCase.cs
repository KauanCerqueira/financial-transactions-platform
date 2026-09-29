using FinancialTransactions.Application.Abstractions.Messaging;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Exceptions;
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
                TransactionDto.From(processed),
                AlreadyProcessed: true);
        }

        var existingEvent = await transactionEvents.GetByEventIdAsync(command.EventId, cancellationToken);

        if (existingEvent is not null)
        {
            return await ReturnExistingEventAsync(existingEvent, cancellationToken);
        }

        var amount = Money.Create(command.Amount);

        var transactionEvent = TransactionEvent.Receive(
            command.EventId,
            command.AccountId,
            command.Type,
            amount,
            command.OccurredAt,
            timeProvider.GetUtcNow());

        try
        {
            await transactionEvents.AddAsync(transactionEvent, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DuplicateEventException)
        {
            var concurrentEvent = await transactionEvents.GetByEventIdAsync(command.EventId, cancellationToken);

            if (concurrentEvent is null)
            {
                throw;
            }

            return await ReturnExistingEventAsync(concurrentEvent, cancellationToken);
        }

        await queue.PublishAsync(command, cancellationToken);

        return new EnqueuedTransaction(command.EventId, TransactionEventStatus.Pending, null, null, AlreadyProcessed: false);
    }

    private async Task<EnqueuedTransaction> ReturnExistingEventAsync(
        TransactionEvent transactionEvent,
        CancellationToken cancellationToken)
    {
        if (transactionEvent.Status == TransactionEventStatus.Pending)
        {
            await queue.PublishAsync(new ProcessTransactionCommand(
                transactionEvent.EventId,
                transactionEvent.AccountId,
                transactionEvent.Type,
                transactionEvent.Amount.Amount,
                transactionEvent.OccurredAt), cancellationToken);
        }

        return new EnqueuedTransaction(
            transactionEvent.EventId,
            transactionEvent.Status,
            transactionEvent.RejectionCode,
            null,
            AlreadyProcessed: true);
    }
}
