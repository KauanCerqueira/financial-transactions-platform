using FinancialTransactions.Application.Features.Accounts;
using FinancialTransactions.Application.Features.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetAccountsUseCase, GetAccountsUseCase>();
        services.AddScoped<IProcessTransactionUseCase, ProcessTransactionUseCase>();
        services.AddScoped<IGetStatementUseCase, GetStatementUseCase>();

        return services;
    }
}
