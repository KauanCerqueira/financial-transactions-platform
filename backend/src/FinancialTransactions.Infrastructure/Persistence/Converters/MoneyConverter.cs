using FinancialTransactions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FinancialTransactions.Infrastructure.Persistence.Converters;

public sealed class MoneyConverter : ValueConverter<Money, decimal>
{
    public MoneyConverter()
        : base(
            money => money.Amount,
            value => Money.Create(value))
    {
    }
}
