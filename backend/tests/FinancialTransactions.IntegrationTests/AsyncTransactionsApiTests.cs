using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class AsyncTransactionsApiTests
{
    private static readonly TimeSpan CompletionTimeout = TimeSpan.FromSeconds(20);

    private readonly PostgresApiFixture _fixture;

    public AsyncTransactionsApiTests(PostgresApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task EnqueueAsync_WithNewEvent_IsAcceptedThenProcessed()
    {
        var accountId = await _fixture.CreateAccountAsync("Enfileirado Teste", 100m);

        var response = await EnqueueAsync(accountId, "CREDIT", 50m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var accepted = await ApiJson.ReadAsync<TransactionAcceptedResponse>(response);
        accepted!.Status.Should().Be(TransactionEventStatus.Pending);

        var completed = await WaitForCompletionAsync(accepted.EventId);

        completed.Status.Should().Be(TransactionEventStatus.Processed);
        completed.Transaction!.BalanceAfter.Should().Be(150m);

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task EnqueueAsync_WithSameEventIdTwice_IsIdempotent()
    {
        var accountId = await _fixture.CreateAccountAsync("Fila Idempotente", 100m);
        var eventId = Guid.NewGuid();

        var first = await EnqueueAsync(accountId, "CREDIT", 50m, eventId);
        var second = await EnqueueAsync(accountId, "CREDIT", 50m, eventId);

        first.StatusCode.Should().Be(HttpStatusCode.Accepted);
        second.StatusCode.Should().BeOneOf(HttpStatusCode.Accepted, HttpStatusCode.OK);

        var secondBody = await ApiJson.ReadAsync<TransactionAcceptedResponse>(second);
        secondBody!.AlreadyProcessed.Should().BeTrue();

        var completed = await WaitForCompletionAsync(eventId);
        completed.Status.Should().Be(TransactionEventStatus.Processed);

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task EnqueueAsync_WithInsufficientFunds_IsRejected()
    {
        var accountId = await _fixture.CreateAccountAsync("Fila sem Saldo", 100m);

        var response = await EnqueueAsync(accountId, "DEBIT", 150m, Guid.NewGuid());
        var accepted = await ApiJson.ReadAsync<TransactionAcceptedResponse>(response);

        var completed = await WaitForCompletionAsync(accepted!.EventId);

        completed.Status.Should().Be(TransactionEventStatus.Rejected);
        completed.RejectionCode.Should().Be("INSUFFICIENT_FUNDS");

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(100m);
    }

    [Fact]
    public async Task GetStatus_ForUnknownEvent_Returns404()
    {
        var response = await _fixture.Client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RepublishPending_WhenEventWasNotPublished_ProcessesStoredEvent()
    {
        var accountId = await _fixture.CreateAccountAsync("Recuperacao Teste", 100m);
        var eventId = await _fixture.CreatePendingEventAsync(accountId);

        await _fixture.RepublishPendingAsync();
        var completed = await WaitForCompletionAsync(eventId);

        completed.Status.Should().Be(TransactionEventStatus.Processed);

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(125m);
    }

    private async Task<TransactionAcceptedResponse> WaitForCompletionAsync(Guid eventId)
    {
        var deadline = DateTime.UtcNow + CompletionTimeout;
        TransactionAcceptedResponse? latest = null;

        while (DateTime.UtcNow < deadline)
        {
            latest = await _fixture.Client.GetFromJsonAsync<TransactionAcceptedResponse>(
                $"/api/transactions/{eventId}", ApiJson.Options);

            if (latest is not null && latest.Status != TransactionEventStatus.Pending)
            {
                return latest;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException($"O evento {eventId} não foi processado a tempo (último status: {latest?.Status}).");
    }

    private Task<HttpResponseMessage> EnqueueAsync(Guid accountId, string type, decimal amount, Guid eventId) =>
        _fixture.Client.PostAsJsonAsync("/api/transactions/async", new
        {
            eventId,
            accountId,
            type,
            amount,
            occurredAt = DateTimeOffset.UtcNow
        }, ApiJson.Options);
}
