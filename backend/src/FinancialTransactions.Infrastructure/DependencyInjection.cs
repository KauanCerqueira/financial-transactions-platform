using FinancialTransactions.Application.Abstractions.Caching;
using FinancialTransactions.Application.Abstractions.Messaging;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Infrastructure.Caching;
using FinancialTransactions.Infrastructure.Messaging;
using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string? redisConnectionString = null,
        RabbitMqOptions? rabbitMqOptions = null)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
        }

        services.AddSingleton<ICacheService, DistributedCacheService>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionEventRepository, TransactionEventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var messaging = rabbitMqOptions ?? new RabbitMqOptions();
        services.AddSingleton(messaging);

        if (messaging.IsConfigured)
        {
            services.AddSingleton<ITransactionQueue, RabbitMqTransactionQueue>();
        }
        else
        {
            services.AddSingleton<ITransactionQueue, DisabledTransactionQueue>();
        }

        return services;
    }
}
