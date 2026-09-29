using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace FinancialTransactions.IntegrationTests;

public sealed class PostgresApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("financial_transactions")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _container.GetConnectionString());

        _factory = new WebApplicationFactory<Program>();
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", null);
    }

    public async Task<Guid> CreateAccountAsync(string holderName, decimal initialBalance)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTimeOffset.UtcNow;

        var account = Account.Create(Guid.NewGuid(), holderName, Money.Zero, now);
        var transaction = account.RegisterTransaction(
            Guid.NewGuid(), TransactionType.Credit, Money.Create(initialBalance), now, now);

        dbContext.Accounts.Add(account);
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();

        return account.Id;
    }
}
