using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransactions.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id).HasColumnName("id");
        builder.Property(transaction => transaction.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(transaction => transaction.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(transaction => transaction.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(transaction => transaction.Amount).HasColumnName("amount").HasConversion<MoneyConverter>().HasPrecision(18, 2).IsRequired();
        builder.Property(transaction => transaction.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(transaction => transaction.BalanceAfter).HasColumnName("balance_after").HasConversion<MoneyConverter>().HasPrecision(18, 2).IsRequired();
        builder.Property(transaction => transaction.RecordedAt).HasColumnName("recorded_at").IsRequired();

        builder.HasIndex(transaction => transaction.EventId)
            .IsUnique()
            .HasDatabaseName("ux_transactions_event_id");

        builder.HasIndex(transaction => new { transaction.AccountId, transaction.OccurredAt })
            .HasDatabaseName("ix_transactions_account_occurred_at");
    }
}
