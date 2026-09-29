using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Exceptions;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed class ProcessTransactionUseCase(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IProcessTransactionUseCase
{
    public async Task<ProcessTransactionResult> ExecuteAsync(
        ProcessTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var processedTransaction = await transactions.GetByEventIdAsync(command.EventId, cancellationToken);

        if (processedTransaction is not null)
        {
            return Replay(processedTransaction);
        }

        var amount = Money.Create(command.Amount);

        try
        {
            return await ProcessNewTransactionAsync(command, amount, cancellationToken);
        }
        catch (DuplicateEventException)
        {
            var concurrentTransaction = await transactions.GetByEventIdAsync(command.EventId, cancellationToken);

            if (concurrentTransaction is null)
            {
                throw;
            }

            return Replay(concurrentTransaction);
        }
    }

    private async Task<ProcessTransactionResult> ProcessNewTransactionAsync(
        ProcessTransactionCommand command,
        Money amount,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var account = await accounts.GetByIdWithLockAsync(command.AccountId, token)
                ?? throw new AccountNotFoundException(command.AccountId);

            var transaction = account.Apply(
                command.EventId,
                command.Type,
                amount,
                command.OccurredAt,
                timeProvider.GetUtcNow());

            await transactions.AddAsync(transaction, token);

            await unitOfWork.SaveChangesAsync(token);

            return new ProcessTransactionResult(TransactionDto.From(transaction), AlreadyProcessed: false);
        }, cancellationToken);
    }

    private static ProcessTransactionResult Replay(Transaction transaction) =>
        new(TransactionDto.From(transaction), AlreadyProcessed: true);
}
