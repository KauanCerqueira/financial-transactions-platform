using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class TransactionsApiTests
{
    private static readonly TimeSpan CompletionTimeout = TimeSpan.FromSeconds(20);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    private readonly PostgresApiFixture _fixture;

    public TransactionsApiTests(PostgresApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task PostTransaction_WithNewEvent_IsAcceptedThenProcessed()
    {
        var accountId = await _fixture.CreateAccountAsync("Credito Teste", 100m);

        var accepted = await PostTransactionAsync(accountId, "CREDIT", 50m, Guid.NewGuid());

        accepted.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var acceptedBody = await ReadAsync<TransactionAcceptedResponse>(accepted);
        acceptedBody!.Status.Should().Be(TransactionEventStatus.Pending);

        var completed = await WaitForCompletionAsync(acceptedBody.EventId);

        completed.Status.Should().Be(TransactionEventStatus.Processed);
        completed.Transaction!.Amount.Should().Be(50m);
        completed.Transaction.BalanceAfter.Should().Be(150m);

        var account = await GetAccountAsync(accountId);
        account!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task PostTransaction_WithSameEventIdTwice_ProcessesOnlyOnce()
    {
        var accountId = await _fixture.CreateAccountAsync("Idempotencia Teste", 100m);
        var eventId = Guid.NewGuid();

        var first = await PostTransactionAsync(accountId, "CREDIT", 50m, eventId);
        var second = await PostTransactionAsync(accountId, "CREDIT", 50m, eventId);

        first.StatusCode.Should().Be(HttpStatusCode.Accepted);
        second.StatusCode.Should().BeOneOf(HttpStatusCode.Accepted, HttpStatusCode.OK);

        var completed = await WaitForCompletionAsync(eventId);
        completed.Status.Should().Be(TransactionEventStatus.Processed);

        var account = await GetAccountAsync(accountId);
        account!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task PostTransaction_WithInsufficientFunds_IsRejected()
    {
        var accountId = await _fixture.CreateAccountAsync("Saldo Teste", 100m);

        var accepted = await PostTransactionAsync(accountId, "DEBIT", 150m, Guid.NewGuid());
        var acceptedBody = await ReadAsync<TransactionAcceptedResponse>(accepted);

        var completed = await WaitForCompletionAsync(acceptedBody!.EventId);

        completed.Status.Should().Be(TransactionEventStatus.Rejected);
        completed.Transaction.Should().BeNull();

        var account = await GetAccountAsync(accountId);
        account!.Balance.Should().Be(100m);
    }

    [Fact]
    public async Task PostTransaction_WithInvalidType_Returns400()
    {
        var accountId = await _fixture.CreateAccountAsync("Tipo Teste", 100m);

        var response = await PostTransactionAsync(accountId, "PIX", 10m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStatus_ForUnknownEvent_Returns404()
    {
        var response = await _fixture.Client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStatement_ForUnknownAccount_Returns404()
    {
        var response = await _fixture.Client.GetAsync($"/api/accounts/{Guid.NewGuid()}/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStatement_ReturnsPaginatedTransactionsWithBalanceAfter()
    {
        var accountId = await _fixture.CreateAccountAsync("Extrato Teste", 100m);
        await PostTransactionAsync(accountId, "CREDIT", 50m, Guid.NewGuid());
        await PostTransactionAsync(accountId, "DEBIT", 30m, Guid.NewGuid());

        await WaitForTotalTransactionsAsync(accountId, 3);

        var statement = await _fixture.Client.GetFromJsonAsync<PagedResult<TransactionDto>>(
            $"/api/accounts/{accountId}/transactions?page=1&pageSize=2", JsonOptions);

        statement!.TotalItems.Should().Be(3);
        statement.Items.Should().HaveCount(2);
        statement.TotalPages.Should().Be(2);
        statement.Items.Should().OnlyContain(transaction => transaction.BalanceAfter >= 0m);
    }

    [Fact]
    public async Task ConcurrentDebits_NeverLeaveBalanceNegative()
    {
        var accountId = await _fixture.CreateAccountAsync("Concorrencia Teste", 100m);

        var first = await PostTransactionAsync(accountId, "DEBIT", 60m, Guid.NewGuid());
        var second = await PostTransactionAsync(accountId, "DEBIT", 60m, Guid.NewGuid());

        var firstBody = await ReadAsync<TransactionAcceptedResponse>(first);
        var secondBody = await ReadAsync<TransactionAcceptedResponse>(second);

        var firstResult = await WaitForCompletionAsync(firstBody!.EventId);
        var secondResult = await WaitForCompletionAsync(secondBody!.EventId);

        var statuses = new[] { firstResult.Status, secondResult.Status };
        statuses.Count(status => status == TransactionEventStatus.Processed).Should().Be(1);
        statuses.Count(status => status == TransactionEventStatus.Rejected).Should().Be(1);

        var account = await GetAccountAsync(accountId);
        account!.Balance.Should().Be(40m);
    }

    [Fact]
    public async Task Health_LivenessAndReadiness_AreHealthy()
    {
        var liveness = await _fixture.Client.GetAsync("/health");
        var readiness = await _fixture.Client.GetAsync("/health/ready");

        liveness.StatusCode.Should().Be(HttpStatusCode.OK);
        readiness.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<TransactionAcceptedResponse> WaitForCompletionAsync(Guid eventId)
    {
        var deadline = DateTime.UtcNow + CompletionTimeout;
        TransactionAcceptedResponse? latest = null;

        while (DateTime.UtcNow < deadline)
        {
            latest = await _fixture.Client.GetFromJsonAsync<TransactionAcceptedResponse>(
                $"/api/transactions/{eventId}", JsonOptions);

            if (latest is not null && latest.Status != TransactionEventStatus.Pending)
            {
                return latest;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException($"O evento {eventId} não foi processado a tempo (último status: {latest?.Status}).");
    }

    private async Task WaitForTotalTransactionsAsync(Guid accountId, int expected)
    {
        var deadline = DateTime.UtcNow + CompletionTimeout;

        while (DateTime.UtcNow < deadline)
        {
            var statement = await _fixture.Client.GetFromJsonAsync<PagedResult<TransactionDto>>(
                $"/api/accounts/{accountId}/transactions?page=1&pageSize=10", JsonOptions);

            if (statement!.TotalItems >= expected)
            {
                return;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException($"A conta {accountId} não alcançou {expected} lançamentos a tempo.");
    }

    private Task<HttpResponseMessage> PostTransactionAsync(Guid accountId, string type, decimal amount, Guid eventId) =>
        _fixture.Client.PostAsJsonAsync("/api/transactions", new
        {
            eventId,
            accountId,
            type,
            amount,
            occurredAt = DateTimeOffset.UtcNow
        }, JsonOptions);

    private async Task<AccountDto?> GetAccountAsync(Guid accountId)
    {
        var accounts = await _fixture.Client.GetFromJsonAsync<List<AccountDto>>("/api/accounts", JsonOptions);

        return accounts!.Single(account => account.Id == accountId);
    }

    private static Task<T?> ReadAsync<T>(HttpResponseMessage response) =>
        response.Content.ReadFromJsonAsync<T>(JsonOptions);
}
