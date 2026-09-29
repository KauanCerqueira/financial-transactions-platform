using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    private static readonly (Guid AccountId, string HolderName, Guid EventId, decimal InitialAmount)[] SeedAccounts =
    [
        (Guid.Parse("11111111-1111-1111-1111-111111111111"), "Ana Souza", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), 1500.00m),
        (Guid.Parse("22222222-2222-2222-2222-222222222222"), "Bruno Lima", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), 800.50m),
        (Guid.Parse("33333333-3333-3333-3333-333333333333"), "Carla Mendes", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), 3200.00m)
    ];

    public static async Task SeedAsync(
        AppDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Accounts.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = timeProvider.GetUtcNow();

        foreach (var seedAccount in SeedAccounts)
        {
            var account = Account.Create(seedAccount.AccountId, seedAccount.HolderName, Money.Zero, now);
            account.RegisterTransaction(seedAccount.EventId, TransactionType.Credit, Money.Create(seedAccount.InitialAmount), now, now);

            await dbContext.Accounts.AddAsync(account, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
