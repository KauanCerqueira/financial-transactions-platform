using FluentAssertions;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Features.Accounts;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.ValueObjects;
using Moq;

namespace FinancialTransactions.UnitTests.Application;

public sealed class GetAccountsUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAllAsync_MapsAccountsToDtos()
    {
        var accounts = new Mock<IAccountRepository>();
        accounts
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account>
            {
                Account.Create(Guid.NewGuid(), "Ana", Money.Create(100m), Now),
                Account.Create(Guid.NewGuid(), "Bruno", Money.Create(250.50m), Now)
            });

        var useCase = new GetAccountsUseCase(accounts.Object);

        var result = await useCase.GetAllAsync();

        result.Should().HaveCount(2);
        result.Select(account => account.HolderName).Should().BeEquivalentTo("Ana", "Bruno");
        result.Sum(account => account.Balance).Should().Be(350.50m);
    }
}
