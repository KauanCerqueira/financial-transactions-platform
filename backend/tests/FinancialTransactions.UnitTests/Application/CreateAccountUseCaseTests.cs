using FluentAssertions;
using FinancialTransactions.Application.Abstractions.Caching;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Features.Accounts;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.Exceptions;
using FinancialTransactions.UnitTests.TestDoubles;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace FinancialTransactions.UnitTests.Application;

public sealed class CreateAccountUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FakeCacheService _cache = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly CreateAccountUseCase _useCase;

    public CreateAccountUseCaseTests()
    {
        _timeProvider.SetUtcNow(Now);

        _useCase = new CreateAccountUseCase(_accounts.Object, _unitOfWork.Object, _cache, _timeProvider);
    }

    [Fact]
    public async Task CreateAsync_WithInitialBalance_OpensAccountWithCreditAndReturnsBalance()
    {
        var result = await _useCase.CreateAsync(new CreateAccountCommand("  Maria Silva  ", 1500.00m));

        result.HolderName.Should().Be("Maria Silva");
        result.Balance.Should().Be(1500.00m);
        result.CreatedAt.Should().Be(Now);

        _accounts.Verify(
            repository => repository.AddAsync(
                It.Is<Account>(account =>
                    account.HolderName == "Maria Silva" &&
                    account.Balance.Amount == 1500.00m &&
                    account.Transactions.Count == 1 &&
                    account.Transactions.Single().Type == TransactionType.Credit &&
                    account.Transactions.Single().Amount.Amount == 1500.00m),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithoutInitialBalance_OpensAccountWithoutTransactions()
    {
        var result = await _useCase.CreateAsync(new CreateAccountCommand("Joana Prado", 0m));

        result.Balance.Should().Be(0m);

        _accounts.Verify(
            repository => repository.AddAsync(
                It.Is<Account>(account => account.Transactions.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithBlankHolderName_ThrowsDomainException()
    {
        var act = () => _useCase.CreateAsync(new CreateAccountCommand("   ", 10m));

        await act.Should().ThrowAsync<DomainException>();

        _accounts.Verify(
            repository => repository.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNegativeInitialBalance_ThrowsDomainException()
    {
        var act = () => _useCase.CreateAsync(new CreateAccountCommand("Maria Silva", -1m));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CreateAsync_InvalidatesTheAccountsListCache()
    {
        await _cache.SetAsync(CacheKeys.AccountsList, new List<object>(), TimeSpan.FromMinutes(1));
        _cache.Contains(CacheKeys.AccountsList).Should().BeTrue();

        await _useCase.CreateAsync(new CreateAccountCommand("Maria Silva", 0m));

        _cache.Contains(CacheKeys.AccountsList).Should().BeFalse();
    }
}
