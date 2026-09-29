using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransactions.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", table =>
            table.HasCheckConstraint("ck_accounts_balance_non_negative", "balance >= 0"));

        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id).HasColumnName("id");
        builder.Property(account => account.HolderName).HasColumnName("holder_name").HasMaxLength(200).IsRequired();
        builder.Property(account => account.Balance).HasColumnName("balance").HasConversion<MoneyConverter>().HasPrecision(18, 2).IsRequired();
        builder.Property(account => account.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasMany(account => account.Transactions)
            .WithOne()
            .HasForeignKey(transaction => transaction.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(account => account.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
