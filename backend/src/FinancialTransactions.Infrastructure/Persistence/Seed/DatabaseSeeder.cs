using System.Security.Cryptography;
using System.Text;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    private sealed record SeedMovement(int DaysAgo, TransactionType Type, decimal Amount);

    private sealed record SeedAccount(Guid AccountId, string HolderName, SeedMovement[] Movements);

    private static readonly SeedAccount[] SeedAccounts =
    [
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Ana Souza",
            [
                new(30, TransactionType.Credit, 1500.00m),
                new(28, TransactionType.Debit, 120.50m),
                new(25, TransactionType.Credit, 3400.00m),
                new(22, TransactionType.Debit, 250.00m),
                new(19, TransactionType.Debit, 89.90m),
                new(16, TransactionType.Credit, 120.00m),
                new(13, TransactionType.Debit, 415.75m),
                new(11, TransactionType.Debit, 62.30m),
                new(9, TransactionType.Credit, 900.00m),
                new(7, TransactionType.Debit, 180.00m),
                new(5, TransactionType.Debit, 74.90m),
                new(3, TransactionType.Credit, 220.00m),
                new(2, TransactionType.Debit, 130.00m),
                new(1, TransactionType.Debit, 55.00m)
            ]),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Bruno Lima",
            [
                new(28, TransactionType.Credit, 800.50m),
                new(20, TransactionType.Credit, 1200.00m),
                new(14, TransactionType.Debit, 320.00m),
                new(9, TransactionType.Debit, 199.99m),
                new(4, TransactionType.Credit, 640.00m),
                new(2, TransactionType.Debit, 88.00m)
            ]),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Carla Mendes",
            [
                new(26, TransactionType.Credit, 3200.00m),
                new(18, TransactionType.Credit, 450.00m),
                new(12, TransactionType.Debit, 1200.00m),
                new(6, TransactionType.Debit, 89.90m),
                new(2, TransactionType.Credit, 300.00m)
            ])
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
        var sequence = 0;

        foreach (var seedAccount in SeedAccounts)
        {
            var account = Account.Create(seedAccount.AccountId, seedAccount.HolderName, Money.Zero, now);

            foreach (var movement in seedAccount.Movements)
            {
                sequence++;
                // histórico de demonstração: eventos do passado, gravados agora
                account.RegisterTransaction(
                    SeedEventId(sequence),
                    movement.Type,
                    Money.Create(movement.Amount),
                    now.AddDays(-movement.DaysAgo),
                    now);
            }

            await dbContext.Accounts.AddAsync(account, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Guid SeedEventId(int sequence) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"seed-transaction-{sequence}"))[..16]);
}
