using FluentAssertions;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.Exceptions;
using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.UnitTests.Domain;

public sealed class AccountTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithEmptyId_ThrowsDomainException()
    {
        var act = () => Account.Create(Guid.Empty, "Ana", Money.Zero, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithWhitespaceHolderName_ThrowsDomainException()
    {
        var act = () => Account.Create(Guid.NewGuid(), "   ", Money.Zero, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterTransaction_WithCredit_IncreasesBalanceAndSetsBalanceAfter()
    {
        var account = Account.Create(Guid.NewGuid(), "Ana", Money.Create(100m), Now);

        var transaction = account.RegisterTransaction(
            Guid.NewGuid(), TransactionType.Credit, Money.Create(50m), Now, Now);

        account.Balance.Should().Be(Money.Create(150m));
        transaction.BalanceAfter.Should().Be(Money.Create(150m));
        account.Transactions.Should().ContainSingle();
    }

    [Fact]
    public void RegisterTransaction_WithDebit_DecreasesBalance()
    {
        var account = Account.Create(Guid.NewGuid(), "Ana", Money.Create(100m), Now);

        account.RegisterTransaction(Guid.NewGuid(), TransactionType.Debit, Money.Create(30m), Now, Now);

        account.Balance.Should().Be(Money.Create(70m));
    }

    [Fact]
    public void RegisterTransaction_WhenDebitExceedsBalance_ThrowsInsufficientFundsException()
    {
        var account = Account.Create(Guid.NewGuid(), "Ana", Money.Create(100m), Now);

        var act = () => account.RegisterTransaction(
            Guid.NewGuid(), TransactionType.Debit, Money.Create(150m), Now, Now);

        act.Should().Throw<InsufficientFundsException>();
    }

    [Fact]
    public void RegisterTransaction_WithZeroAmount_ThrowsDomainException()
    {
        var account = Account.Create(Guid.NewGuid(), "Ana", Money.Create(100m), Now);

        var act = () => account.RegisterTransaction(
            Guid.NewGuid(), TransactionType.Credit, Money.Zero, Now, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterTransaction_WithEmptyEventId_ThrowsDomainException()
    {
        var account = Account.Create(Guid.NewGuid(), "Ana", Money.Create(100m), Now);

        var act = () => account.RegisterTransaction(
            Guid.Empty, TransactionType.Credit, Money.Create(10m), Now, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterTransaction_WithDefaultOccurredAt_ThrowsDomainException()
    {
        var account = Account.Create(Guid.NewGuid(), "Ana", Money.Create(100m), Now);

        var act = () => account.RegisterTransaction(
            Guid.NewGuid(), TransactionType.Credit, Money.Create(10m), default, Now);

        act.Should().Throw<DomainException>();
    }
}
