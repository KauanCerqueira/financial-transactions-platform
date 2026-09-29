using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class AccountsApiTests
{
    private readonly PostgresApiFixture _fixture;

    public AccountsApiTests(PostgresApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task PostAccount_WithInitialBalance_AppearsInListWithOpeningTransaction()
    {
        var request = new CreateAccountRequest { HolderName = "Nova Conta Teste", InitialBalance = 250m };

        var response = await _fixture.Client.PostAsJsonAsync("/api/accounts", request, ApiJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ApiJson.ReadAsync<AccountDto>(response);
        created!.HolderName.Should().Be("Nova Conta Teste");
        created.Balance.Should().Be(250m);

        var accounts = await _fixture.Client.GetFromJsonAsync<List<AccountDto>>("/api/accounts", ApiJson.Options);
        accounts.Should().Contain(account => account.Id == created.Id);

        var statement = await _fixture.Client.GetFromJsonAsync<StatementDto>(
            $"/api/accounts/{created.Id}/transactions", ApiJson.Options);

        statement!.Page.TotalItems.Should().Be(1);
        statement.Page.Items[0].Amount.Should().Be(250m);
        statement.Page.Items[0].BalanceAfter.Should().Be(250m);
        statement.Summary.CreditTotal.Should().Be(250m);
    }

    [Fact]
    public async Task PostAccount_WithoutInitialBalance_StartsAtZeroWithoutTransactions()
    {
        var request = new CreateAccountRequest { HolderName = "Conta Zerada" };

        var response = await _fixture.Client.PostAsJsonAsync("/api/accounts", request, ApiJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ApiJson.ReadAsync<AccountDto>(response);
        created!.Balance.Should().Be(0m);

        var statement = await _fixture.Client.GetFromJsonAsync<StatementDto>(
            $"/api/accounts/{created.Id}/transactions", ApiJson.Options);

        statement!.Page.TotalItems.Should().Be(0);
    }

    [Theory]
    [InlineData("   ", 0)]
    [InlineData("Conta Negativa", -10)]
    public async Task PostAccount_WithInvalidData_ReturnsBadRequest(string holderName, decimal initialBalance)
    {
        var request = new CreateAccountRequest { HolderName = holderName, InitialBalance = initialBalance };

        var response = await _fixture.Client.PostAsJsonAsync("/api/accounts", request, ApiJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
