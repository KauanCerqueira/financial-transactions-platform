using FluentAssertions;
using FinancialTransactions.Application.Abstractions.Caching;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Exceptions;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.Exceptions;
using FinancialTransactions.Domain.ValueObjects;
using FinancialTransactions.UnitTests.TestDoubles;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace FinancialTransactions.UnitTests.Application;

public sealed class ProcessTransactionUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset OccurredAt = new(2026, 1, 30, 10, 15, 0, TimeSpan.Zero);

    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FakeCacheService _cache = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly ProcessTransactionUseCase _useCase;

    public ProcessTransactionUseCaseTests()
    {
        _timeProvider.SetUtcNow(Now);

        _unitOfWork
            .Setup(unit => unit.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<ProcessTransactionResult>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<ProcessTransactionResult>> operation, CancellationToken token) =>
                operation(token));

        _useCase = new ProcessTransactionUseCase(
            _accounts.Object,
            _transactions.Object,
            _unitOfWork.Object,
            _cache,
            _timeProvider);
    }

    [Fact]
    public async Task ProcessAsync_WhenEventAlreadyProcessed_ReturnsAlreadyProcessedResult()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId, TransactionType.Credit, 50m);
        var existing = BuildTransaction(accountId, command.EventId);

        _transactions
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _useCase.ProcessAsync(command);

        result.AlreadyProcessed.Should().BeTrue();
        result.Transaction.Id.Should().Be(existing.Id);
        _accounts.Verify(
            repository => repository.GetByIdWithLockAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenNewEvent_RegistersTransactionAndSaves()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId, TransactionType.Credit, 50m);
        var account = Account.Create(accountId, "Ana", Money.Create(100m), Now);

        SetUpNewEvent(accountId, command, account);

        var result = await _useCase.ProcessAsync(command);

        result.AlreadyProcessed.Should().BeFalse();
        account.Balance.Should().Be(Money.Create(150m));
        _transactions.Verify(
            repository => repository.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenNewEvent_InvalidatesTheCache()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId, TransactionType.Credit, 50m);
        var account = Account.Create(accountId, "Ana", Money.Create(100m), Now);

        await _cache.SetAsync(CacheKeys.AccountsList, new List<string>(), TimeSpan.FromMinutes(1));
        SetUpNewEvent(accountId, command, account);

        await _useCase.ProcessAsync(command);

        _cache.Contains(CacheKeys.AccountsList).Should().BeFalse();
        _cache.Contains(CacheKeys.StatementVersion(accountId)).Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_WhenAccountDoesNotExist_ThrowsAccountNotFoundException()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId, TransactionType.Credit, 50m);

        _transactions
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);
        _accounts
            .Setup(repository => repository.GetByIdWithLockAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var act = () => _useCase.ProcessAsync(command);

        await act.Should().ThrowAsync<AccountNotFoundException>();
    }

    [Fact]
    public async Task ProcessAsync_WhenDebitExceedsBalance_ThrowsInsufficientFundsException()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId, TransactionType.Debit, 150m);
        var account = Account.Create(accountId, "Ana", Money.Create(100m), Now);

        _transactions
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);
        _accounts
            .Setup(repository => repository.GetByIdWithLockAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var act = () => _useCase.ProcessAsync(command);

        await act.Should().ThrowAsync<InsufficientFundsException>();
    }

    [Fact]
    public async Task ProcessAsync_WhenConcurrentInsertHappens_ReturnsAlreadyProcessedResult()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId, TransactionType.Credit, 50m);
        var account = Account.Create(accountId, "Ana", Money.Create(100m), Now);
        var concurrent = BuildTransaction(accountId, command.EventId);

        _transactions
            .SetupSequence(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null)
            .ReturnsAsync(concurrent);
        _accounts
            .Setup(repository => repository.GetByIdWithLockAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _unitOfWork
            .Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateEventException(new InvalidOperationException("unique violation")));

        var result = await _useCase.ProcessAsync(command);

        result.AlreadyProcessed.Should().BeTrue();
        result.Transaction.Id.Should().Be(concurrent.Id);
    }

    private void SetUpNewEvent(Guid accountId, ProcessTransactionCommand command, Account account)
    {
        _transactions
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);
        _accounts
            .Setup(repository => repository.GetByIdWithLockAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
    }

    private static ProcessTransactionCommand Command(Guid accountId, TransactionType type, decimal amount) =>
        new(Guid.NewGuid(), accountId, type, amount, OccurredAt);

    private static Transaction BuildTransaction(Guid accountId, Guid eventId)
    {
        var account = Account.Create(accountId, "Ana", Money.Zero, Now);

        return account.RegisterTransaction(eventId, TransactionType.Credit, Money.Create(100m), Now, Now);
    }
}
