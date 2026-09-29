using System.Globalization;
using FinancialTransactions.Domain.Exceptions;

namespace FinancialTransactions.Domain.ValueObjects;

public sealed class Money : IEquatable<Money>, IComparable<Money>
{
    public const int Scale = 2;

    public decimal Amount { get; }

    public static Money Zero { get; } = new(0m);

    private Money(decimal amount) => Amount = amount;

    public static Money Create(decimal amount)
    {
        if (amount < 0)
        {
            throw new DomainException("O valor monetário não pode ser negativo.");
        }

        // arredondamento bancário: metade para o par
        return new Money(Math.Round(amount, Scale, MidpointRounding.ToEven));
    }

    public bool IsZero => Amount == 0m;

    public Money Add(Money other) => Create(Amount + other.Amount);

    public Money Subtract(Money other)
    {
        var result = Amount - other.Amount;

        if (result < 0)
        {
            throw new DomainException("A operação resultaria em um valor monetário negativo.");
        }

        return Create(result);
    }

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;

    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

    public static bool operator ==(Money? left, Money? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public int CompareTo(Money? other) => other is null ? 1 : Amount.CompareTo(other.Amount);

    public bool Equals(Money? other) => other is not null && Amount == other.Amount;

    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    public override int GetHashCode() => Amount.GetHashCode();

    public override string ToString() => Amount.ToString("0.00", CultureInfo.InvariantCulture);
}
