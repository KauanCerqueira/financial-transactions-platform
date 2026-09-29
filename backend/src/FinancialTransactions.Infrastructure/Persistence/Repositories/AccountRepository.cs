using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(AppDbContext dbContext) : IAccountRepository
{
    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Accounts
            .AsNoTracking()
            .OrderBy(account => account.HolderName)
            .ToListAsync(cancellationToken);

    public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default) =>
        dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Id == accountId, cancellationToken);

    public async Task<Account?> GetByIdWithLockAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var lockedAccounts = await dbContext.Accounts
            .FromSql($"SELECT * FROM accounts WHERE id = {accountId} FOR UPDATE")
            .ToListAsync(cancellationToken);

        return lockedAccounts.FirstOrDefault();
    }
}
