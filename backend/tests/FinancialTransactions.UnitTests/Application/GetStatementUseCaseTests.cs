using FluentAssertions;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Exceptions;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;
using FinancialTransactions.UnitTests.TestDoubles;
using Moq;

namespace FinancialTransactions.UnitTests.Application;

public sealed class GetStatementUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly FakeCacheService _cache = new();
    private readonly GetStatementUseCase _useCase;

    public GetStatementUseCaseTests() =>
        _useCase = new GetStatementUseCase(_accounts.Object, _transactions.Object, _cache);

    [Fact]
    public async Task GetStatementAsync_WhenAccountDoesNotExist_ThrowsAccountNotFoundException()
    {
        var accountId = Guid.NewGuid();

        _accounts
            .Setup(repository => repository.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var act = () => _useCase.GetStatementAsync(new GetStatementQuery(accountId, 1, 20));

        await act.Should().ThrowAsync<AccountNotFoundException>();
    }

    [Fact]
    public async Task GetStatementAsync_WhenAccountExists_ReturnsPageAndSummary()
    {
        var accountId = Guid.NewGuid();

        SetUpExistingAccountWithOneTransaction(accountId, BuildTransaction(accountId));

        var result = await _useCase.GetStatementAsync(new GetStatementQuery(accountId, 1, 20));

        result.Page.Items.Should().ContainSingle();
        result.Page.Items[0].BalanceAfter.Should().Be(100m);
        result.Page.Page.Should().Be(1);
        result.Page.PageSize.Should().Be(20);
        result.Page.TotalItems.Should().Be(1);
        result.Page.TotalPages.Should().Be(1);
        result.Summary.CreditTotal.Should().Be(100m);
    }

    [Fact]
    public async Task GetStatementAsync_OnSecondCall_UsesTheCache()
    {
        var accountId = Guid.NewGuid();

        SetUpExistingAccountWithOneTransaction(accountId, BuildTransaction(accountId));

        await _useCase.GetStatementAsync(new GetStatementQuery(accountId, 1, 20));
        await _useCase.GetStatementAsync(new GetStatementQuery(accountId, 1, 20));

        _transactions.Verify(
            repository => repository.GetPageAsync(
                accountId, It.IsAny<StatementFilter>(), 0, 20, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatementAsync_WithFilters_PassesThemToTheRepository()
    {
        var accountId = Guid.NewGuid();
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);

        SetUpExistingAccountWithOneTransaction(accountId, BuildTransaction(accountId));

        await _useCase.GetStatementAsync(
            new GetStatementQuery(accountId, 1, 20, TransactionType.Debit, from, to));

        _transactions.Verify(
            repository => repository.GetPageAsync(
                accountId,
                It.Is<StatementFilter>(filter => filter.Type == TransactionType.Debit && filter.From == from && filter.To == to),
                0,
                20,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _transactions.Verify(
            repository => repository.SummarizeAsync(
                accountId,
                It.Is<StatementFilter>(filter => filter.Type == TransactionType.Debit),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatementAsync_WithInvalidPaging_ClampsPageAndPageSize()
    {
        var accountId = Guid.NewGuid();

        SetUpAccount(accountId);
        _transactions
            .Setup(repository => repository.GetPageAsync(
                accountId, It.IsAny<StatementFilter>(), 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Transaction>());
        _transactions
            .Setup(repository => repository.CountByAccountIdAsync(
                accountId, It.IsAny<StatementFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        SetUpSummarize(accountId);

        var result = await _useCase.GetStatementAsync(new GetStatementQuery(accountId, 0, 1000));

        result.Page.Page.Should().Be(1);
        result.Page.PageSize.Should().Be(100);
        _transactions.Verify(
            repository => repository.GetPageAsync(
                accountId, It.IsAny<StatementFilter>(), 0, 100, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetUpExistingAccountWithOneTransaction(Guid accountId, Transaction transaction)
    {
        SetUpAccount(accountId);
        _transactions
            .Setup(repository => repository.GetPageAsync(
                accountId, It.IsAny<StatementFilter>(), 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Transaction> { transaction });
        _transactions
            .Setup(repository => repository.CountByAccountIdAsync(
                accountId, It.IsAny<StatementFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        SetUpSummarize(accountId);
    }

    private void SetUpAccount(Guid accountId) =>
        _accounts
            .Setup(repository => repository.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Account.Create(accountId, "Ana", Money.Zero, Now));

    private void SetUpSummarize(Guid accountId) =>
        _transactions
            .Setup(repository => repository.SummarizeAsync(
                accountId, It.IsAny<StatementFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatementSummaryDto(1, 100m, 0, 0m));

    private static Transaction BuildTransaction(Guid accountId)
    {
        var account = Account.Create(accountId, "Ana", Money.Zero, Now);

        return account.RegisterTransaction(Guid.NewGuid(), TransactionType.Credit, Money.Create(100m), Now, Now);
    }
}
