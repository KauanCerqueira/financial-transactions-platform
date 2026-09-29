using FluentAssertions;
using FinancialTransactions.Application.Abstractions.Messaging;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace FinancialTransactions.UnitTests.Application;

public sealed class EnqueueTransactionUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<ITransactionEventRepository> _transactionEvents = new();
    private readonly Mock<ITransactionQueue> _queue = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly EnqueueTransactionUseCase _useCase;

    public EnqueueTransactionUseCaseTests()
    {
        _timeProvider.SetUtcNow(Now);
        _queue.SetupGet(queue => queue.IsEnabled).Returns(true);

        _useCase = new EnqueueTransactionUseCase(
            _transactions.Object,
            _transactionEvents.Object,
            _queue.Object,
            _unitOfWork.Object,
            _timeProvider);
    }

    [Fact]
    public async Task EnqueueAsync_WhenNewEvent_StoresPendingAndPublishes()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId);

        _transactions
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);
        _transactionEvents
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionEvent?)null);

        var result = await _useCase.EnqueueAsync(command);

        result.Status.Should().Be(TransactionEventStatus.Pending);
        _transactionEvents.Verify(
            repository => repository.AddAsync(It.IsAny<TransactionEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _queue.Verify(queue => queue.PublishAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnqueueAsync_WhenEventAlreadyProcessed_ReturnsTheTransactionWithoutPublishing()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId);
        var processed = BuildTransaction(accountId, command.EventId);

        _transactions
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(processed);

        var result = await _useCase.EnqueueAsync(command);

        result.Status.Should().Be(TransactionEventStatus.Processed);
        result.Transaction!.Id.Should().Be(processed.Id);
        _queue.Verify(queue => queue.PublishAsync(It.IsAny<ProcessTransactionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnqueueAsync_WhenEventAlreadyEnqueued_ReturnsItsCurrentStatus()
    {
        var accountId = Guid.NewGuid();
        var command = Command(accountId);
        var rejected = TransactionEvent.Receive(command.EventId, accountId, TransactionType.Debit, Money.Create(10m), Now, Now);
        rejected.MarkRejected("INSUFFICIENT_FUNDS", Now);

        _transactions
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);
        _transactionEvents
            .Setup(repository => repository.GetByEventIdAsync(command.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rejected);

        var result = await _useCase.EnqueueAsync(command);

        result.Status.Should().Be(TransactionEventStatus.Rejected);
        _queue.Verify(queue => queue.PublishAsync(It.IsAny<ProcessTransactionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ProcessTransactionCommand Command(Guid accountId) =>
        new(Guid.NewGuid(), accountId, TransactionType.Credit, 100m, Now);

    private static Transaction BuildTransaction(Guid accountId, Guid eventId)
    {
        var account = Account.Create(accountId, "Ana", Money.Zero, Now);

        return account.RegisterTransaction(eventId, TransactionType.Credit, Money.Create(100m), Now, Now);
    }
}
