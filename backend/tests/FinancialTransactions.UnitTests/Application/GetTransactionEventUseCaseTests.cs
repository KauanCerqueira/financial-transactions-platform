using FluentAssertions;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;
using Moq;

namespace FinancialTransactions.UnitTests.Application;

public sealed class GetTransactionEventUseCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 30, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<ITransactionEventRepository> _transactionEvents = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly GetTransactionEventUseCase _useCase;

    public GetTransactionEventUseCaseTests() =>
        _useCase = new GetTransactionEventUseCase(_transactionEvents.Object, _transactions.Object);

    [Fact]
    public async Task GetAsync_WhenEventDoesNotExist_ReturnsNull()
    {
        var eventId = Guid.NewGuid();

        _transactionEvents
            .Setup(repository => repository.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionEvent?)null);

        var result = await _useCase.GetAsync(eventId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenEventIsPending_ReturnsPendingWithoutTransaction()
    {
        var accountId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var pending = TransactionEvent.Receive(eventId, accountId, TransactionType.Credit, Money.Create(50m), Now, Now);

        _transactionEvents
            .Setup(repository => repository.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pending);

        var result = await _useCase.GetAsync(eventId);

        result!.Status.Should().Be(TransactionEventStatus.Pending);
        result.Transaction.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenEventIsProcessed_ReturnsTheTransaction()
    {
        var accountId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var processedEvent = TransactionEvent.Receive(eventId, accountId, TransactionType.Credit, Money.Create(50m), Now, Now);
        processedEvent.MarkProcessed(Now);
        var transaction = BuildTransaction(accountId, eventId);

        _transactionEvents
            .Setup(repository => repository.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(processedEvent);
        _transactions
            .Setup(repository => repository.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await _useCase.GetAsync(eventId);

        result!.Status.Should().Be(TransactionEventStatus.Processed);
        result.Transaction!.Id.Should().Be(transaction.Id);
    }

    [Fact]
    public async Task GetAsync_WhenEventIsRejected_ReturnsRejectedWithoutTransaction()
    {
        var accountId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var rejectedEvent = TransactionEvent.Receive(eventId, accountId, TransactionType.Debit, Money.Create(50m), Now, Now);
        rejectedEvent.MarkRejected("INSUFFICIENT_FUNDS", Now);

        _transactionEvents
            .Setup(repository => repository.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rejectedEvent);

        var result = await _useCase.GetAsync(eventId);

        result!.Status.Should().Be(TransactionEventStatus.Rejected);
        result.Transaction.Should().BeNull();
    }

    private static Transaction BuildTransaction(Guid accountId, Guid eventId)
    {
        var account = Account.Create(accountId, "Ana", Money.Zero, Now);

        return account.RegisterTransaction(eventId, TransactionType.Credit, Money.Create(50m), Now, Now);
    }
}
