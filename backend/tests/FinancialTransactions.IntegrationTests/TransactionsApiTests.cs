using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class TransactionsApiTests
{
    private readonly PostgresApiFixture _fixture;

    public TransactionsApiTests(PostgresApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task PostTransaction_WithNewEvent_CreatesAndUpdatesBalance()
    {
        var accountId = await _fixture.CreateAccountAsync("Credito Teste", 100m);

        var response = await PostTransactionAsync(accountId, "CREDIT", 50m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ApiJson.ReadAsync<TransactionAcceptedResponse>(response);
        body!.Status.Should().Be(TransactionEventStatus.Processed);
        body.AlreadyProcessed.Should().BeFalse();
        body.Transaction!.Amount.Should().Be(50m);
        body.Transaction.BalanceAfter.Should().Be(150m);

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task PostTransaction_WithSameEventIdTwice_ProcessesOnlyOnce()
    {
        var accountId = await _fixture.CreateAccountAsync("Idempotencia Teste", 100m);
        var eventId = Guid.NewGuid();

        var first = await PostTransactionAsync(accountId, "CREDIT", 50m, eventId);
        var second = await PostTransactionAsync(accountId, "CREDIT", 50m, eventId);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondBody = await ApiJson.ReadAsync<TransactionAcceptedResponse>(second);
        secondBody!.AlreadyProcessed.Should().BeTrue();

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task PostTransaction_WithConcurrentDuplicateRequests_ProcessesOnlyOnce()
    {
        var accountId = await _fixture.CreateAccountAsync("Idempotencia Concorrente", 100m);
        var eventId = Guid.NewGuid();

        var responses = await Task.WhenAll(
            PostTransactionAsync(accountId, "CREDIT", 50m, eventId),
            PostTransactionAsync(accountId, "CREDIT", 50m, eventId));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(1);
        responses.Should().OnlyContain(response =>
            response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task PostTransaction_WithInsufficientFunds_Returns422()
    {
        var accountId = await _fixture.CreateAccountAsync("Saldo Teste", 100m);

        var response = await PostTransactionAsync(accountId, "DEBIT", 150m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var error = await ApiJson.ReadAsync<ProblemResponse>(response);
        error!.Code.Should().Be("INSUFFICIENT_FUNDS");

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(100m);
    }

    [Fact]
    public async Task PostTransaction_WithUnknownAccount_Returns404()
    {
        var response = await PostTransactionAsync(Guid.NewGuid(), "CREDIT", 10m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostTransaction_WithInvalidType_Returns400()
    {
        var accountId = await _fixture.CreateAccountAsync("Tipo Teste", 100m);

        var response = await PostTransactionAsync(accountId, "PIX", 10m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConcurrentDebits_NeverLeaveBalanceNegative()
    {
        var accountId = await _fixture.CreateAccountAsync("Concorrencia Teste", 100m);

        var responses = await Task.WhenAll(
            PostTransactionAsync(accountId, "DEBIT", 60m, Guid.NewGuid()),
            PostTransactionAsync(accountId, "DEBIT", 60m, Guid.NewGuid()));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.UnprocessableEntity).Should().Be(1);

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(40m);
    }

    [Fact]
    public async Task ProcessDebit_WithTwoDatabaseScopes_AllowsOnlyOneDebit()
    {
        var accountId = await _fixture.CreateAccountAsync("Disputa real", 100m);

        var results = await Task.WhenAll(
            _fixture.ProcessDebitAsync(accountId, 60m),
            _fixture.ProcessDebitAsync(accountId, 60m));

        results.Should().ContainSingle(result => result);

        var account = await ApiJson.GetAccountAsync(_fixture.Client, accountId);
        account!.Balance.Should().Be(40m);
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

        var statement = await _fixture.Client.GetFromJsonAsync<StatementDto>(
            $"/api/accounts/{accountId}/transactions?page=1&pageSize=2", ApiJson.Options);

        statement!.Page.TotalItems.Should().Be(3);
        statement.Page.Items.Should().HaveCount(2);
        statement.Page.TotalPages.Should().Be(2);
        statement.Page.Items.Should().OnlyContain(transaction => transaction.BalanceAfter >= 0m);
        statement.Summary.CreditTotal.Should().Be(150m);
        statement.Summary.DebitTotal.Should().Be(30m);
    }

    [Fact]
    public async Task GetAccounts_SummaryCountsTransactions()
    {
        var accountId = await _fixture.CreateAccountAsync("Resumo Teste", 100m);
        await PostTransactionAsync(accountId, "CREDIT", 50m, Guid.NewGuid());

        var summary = await _fixture.Client.GetFromJsonAsync<AccountsSummaryDto>("/api/accounts/summary", ApiJson.Options);

        summary!.Accounts.Should().BeGreaterThan(0);
        summary.Transactions.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Health_LivenessAndReadiness_AreHealthy()
    {
        var liveness = await _fixture.Client.GetAsync("/health");
        var readiness = await _fixture.Client.GetAsync("/health/ready");

        liveness.StatusCode.Should().Be(HttpStatusCode.OK);
        readiness.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private Task<HttpResponseMessage> PostTransactionAsync(Guid accountId, string type, decimal amount, Guid eventId) =>
        _fixture.Client.PostAsJsonAsync("/api/transactions", new
        {
            eventId,
            accountId,
            type,
            amount,
            occurredAt = DateTimeOffset.UtcNow
        }, ApiJson.Options);

    private sealed record ProblemResponse(string? Title, int Status, string? Code);
}
