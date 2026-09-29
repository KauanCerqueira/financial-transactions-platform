using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class TransactionsApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    private readonly PostgresApiFixture _fixture;

    public TransactionsApiTests(PostgresApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task PostTransaction_WithNewEvent_ReturnsCreatedAndUpdatesBalance()
    {
        var accountId = await _fixture.CreateAccountAsync("Credito Teste", 100m);

        var response = await PostTransactionAsync(accountId, "CREDIT", 50m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var transaction = await ReadAsync<TransactionDto>(response);
        transaction!.Amount.Should().Be(50m);
        transaction.BalanceAfter.Should().Be(150m);

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

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstTransaction = await ReadAsync<TransactionDto>(first);
        var secondTransaction = await ReadAsync<TransactionDto>(second);
        secondTransaction!.Id.Should().Be(firstTransaction!.Id);

        var account = await GetAccountAsync(accountId);
        account!.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task PostTransaction_WithInsufficientFunds_Returns422WithCode()
    {
        var accountId = await _fixture.CreateAccountAsync("Saldo Teste", 100m);

        var response = await PostTransactionAsync(accountId, "DEBIT", 150m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var error = await ReadAsync<ApiError>(response);
        error!.Code.Should().Be("INSUFFICIENT_FUNDS");
    }

    [Fact]
    public async Task PostTransaction_WithInvalidType_Returns400()
    {
        var accountId = await _fixture.CreateAccountAsync("Tipo Teste", 100m);

        var response = await PostTransactionAsync(accountId, "PIX", 10m, Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

        var responses = await Task.WhenAll(
            PostTransactionAsync(accountId, "DEBIT", 60m, Guid.NewGuid()),
            PostTransactionAsync(accountId, "DEBIT", 60m, Guid.NewGuid()));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.UnprocessableEntity).Should().Be(1);

        var account = await GetAccountAsync(accountId);
        account!.Balance.Should().Be(40m);
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

    private sealed record ApiError(string? Title, int Status, string? Code);
}
