using FluentAssertions;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Features.Accounts;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.ValueObjects;
using Moq;

namespace FinancialTransactions.UnitTests.Application;

public sealed class GetAccountsSummaryUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_ReturnsAccountsTotalBalanceAndTransactionCount()
    {
        var accounts = new Mock<IAccountRepository>();
        accounts
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account>
            {
                Account.Create(Guid.NewGuid(), "Ana", Money.Create(100m), Now),
                Account.Create(Guid.NewGuid(), "Bruno", Money.Create(250.50m), Now)
            });

        var transactions = new Mock<ITransactionRepository>();
        transactions
            .Setup(repository => repository.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var useCase = new GetAccountsSummaryUseCase(accounts.Object, transactions.Object);

        var result = await useCase.ExecuteAsync();

        result.Accounts.Should().Be(2);
        result.TotalBalance.Should().Be(350.50m);
        result.Transactions.Should().Be(7);
    }
}
