using FluentAssertions;
using FinancialTransactions.Domain.Exceptions;
using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.UnitTests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithNegativeAmount_ThrowsDomainException()
    {
        var act = () => Money.Create(-1m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_RoundsToTwoDecimalPlaces()
    {
        Money.Create(10.121m).Amount.Should().Be(10.12m);
        Money.Create(10.129m).Amount.Should().Be(10.13m);
    }

    [Fact]
    public void Add_ReturnsSum()
    {
        var result = Money.Create(10.50m).Add(Money.Create(2.25m));

        result.Should().Be(Money.Create(12.75m));
    }

    [Fact]
    public void Subtract_WithSufficientAmount_ReturnsDifference()
    {
        var result = Money.Create(10m).Subtract(Money.Create(4.50m));

        result.Should().Be(Money.Create(5.50m));
    }

    [Fact]
    public void Subtract_WhenResultWouldBeNegative_ThrowsDomainException()
    {
        var act = () => Money.Create(10m).Subtract(Money.Create(20m));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_WithSameAmount_IsTrue()
    {
        Money.Create(10m).Should().Be(Money.Create(10m));
    }
}
