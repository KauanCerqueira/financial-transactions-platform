using FluentAssertions;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Exceptions;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;
using Moq;

namespace FinancialTransactions.UnitTests.Application;

public sealed class GetStatementUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly GetStatementUseCase _useCase;

    public GetStatementUseCaseTests() =>
        _useCase = new GetStatementUseCase(_accounts.Object, _transactions.Object);

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
    public async Task GetStatementAsync_WhenAccountExists_ReturnsPagedResultWithItems()
    {
        var accountId = Guid.NewGuid();
        var account = Account.Create(accountId, "Ana", Money.Zero, Now);
        var transaction = account.RegisterTransaction(
            Guid.NewGuid(), TransactionType.Credit, Money.Create(100m), Now, Now);

        _accounts
            .Setup(repository => repository.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _transactions
            .Setup(repository => repository.GetPageAsync(accountId, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Transaction> { transaction });
        _transactions
            .Setup(repository => repository.CountByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _useCase.GetStatementAsync(new GetStatementQuery(accountId, 1, 20));

        result.Items.Should().ContainSingle();
        result.Items[0].BalanceAfter.Should().Be(100m);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalItems.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetStatementAsync_WithInvalidPaging_ClampsPageAndPageSize()
    {
        var accountId = Guid.NewGuid();
        var account = Account.Create(accountId, "Ana", Money.Zero, Now);

        _accounts
            .Setup(repository => repository.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _transactions
            .Setup(repository => repository.GetPageAsync(accountId, 0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Transaction>());
        _transactions
            .Setup(repository => repository.CountByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _useCase.GetStatementAsync(new GetStatementQuery(accountId, 0, 1000));

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(100);
        _transactions.Verify(
            repository => repository.GetPageAsync(accountId, 0, 100, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
