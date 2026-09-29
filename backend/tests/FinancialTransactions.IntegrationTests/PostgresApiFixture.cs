using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Application.Abstractions.Caching;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Exceptions;
using FinancialTransactions.Domain.Enums;
using FinancialTransactions.Domain.ValueObjects;
using FinancialTransactions.Infrastructure.Messaging;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace FinancialTransactions.IntegrationTests;

public sealed class PostgresApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("financial_transactions")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3.13-management").Build();

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbitMq.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("RabbitMq__Uri", _rabbitMq.GetConnectionString());
        Environment.SetEnvironmentVariable("RabbitMq__QueueName", "transactions-tests");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => services.AddHostedService<TransactionQueueConsumer>()));

        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await _postgres.DisposeAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__Default", null);
        Environment.SetEnvironmentVariable("RabbitMq__Uri", null);
        Environment.SetEnvironmentVariable("RabbitMq__QueueName", null);
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
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        await cache.RemoveAsync(CacheKeys.AccountsList);

        return account.Id;
    }

    public async Task<bool> ProcessDebitAsync(Guid accountId, decimal amount)
    {
        using var scope = _factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IProcessTransactionUseCase>();
        var command = new ProcessTransactionCommand(
            Guid.NewGuid(), accountId, TransactionType.Debit, amount, DateTimeOffset.UtcNow);

        try
        {
            await processor.ProcessAsync(command);
            return true;
        }
        catch (InsufficientFundsException)
        {
            return false;
        }
    }

    public async Task<Guid> CreatePendingEventAsync(Guid accountId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventId = Guid.NewGuid();
        var receivedAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var pending = TransactionEvent.Receive(
            eventId, accountId, TransactionType.Credit, Money.Create(25m), receivedAt, receivedAt);

        dbContext.TransactionEvents.Add(pending);
        await dbContext.SaveChangesAsync();

        return eventId;
    }

    public async Task RepublishPendingAsync()
    {
        using var republisher = ActivatorUtilities.CreateInstance<PendingTransactionRepublisher>(_factory.Services);
        await republisher.RepublishPendingAsync();
    }
}
