using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransactions.Infrastructure.Persistence.Configurations;

public sealed class TransactionEventConfiguration : IEntityTypeConfiguration<TransactionEvent>
{
    public void Configure(EntityTypeBuilder<TransactionEvent> builder)
    {
        builder.ToTable("transaction_events");

        builder.HasKey(transactionEvent => transactionEvent.EventId);

        builder.Property(transactionEvent => transactionEvent.EventId).HasColumnName("event_id");
        builder.Property(transactionEvent => transactionEvent.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(transactionEvent => transactionEvent.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(transactionEvent => transactionEvent.Amount).HasColumnName("amount").HasConversion<MoneyConverter>().HasPrecision(18, 2).IsRequired();
        builder.Property(transactionEvent => transactionEvent.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(transactionEvent => transactionEvent.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(12).IsRequired();
        builder.Property(transactionEvent => transactionEvent.RejectionCode).HasColumnName("rejection_code").HasMaxLength(40);
        builder.Property(transactionEvent => transactionEvent.ReceivedAt).HasColumnName("received_at").IsRequired();
        builder.Property(transactionEvent => transactionEvent.ProcessedAt).HasColumnName("processed_at");

        builder.HasIndex(transactionEvent => new { transactionEvent.AccountId, transactionEvent.ReceivedAt })
            .HasDatabaseName("ix_transaction_events_account_received_at");
    }
}
