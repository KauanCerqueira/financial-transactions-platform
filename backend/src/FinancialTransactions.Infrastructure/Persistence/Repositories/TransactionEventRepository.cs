using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence.Repositories;

public sealed class TransactionEventRepository(AppDbContext dbContext) : ITransactionEventRepository
{
    public async Task AddAsync(TransactionEvent transactionEvent, CancellationToken cancellationToken = default) =>
        await dbContext.TransactionEvents.AddAsync(transactionEvent, cancellationToken);

    public Task<TransactionEvent?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        dbContext.TransactionEvents.FirstOrDefaultAsync(transactionEvent => transactionEvent.EventId == eventId, cancellationToken);
}
