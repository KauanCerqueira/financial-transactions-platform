using FinancialTransactions.Application.Abstractions.Caching;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Accounts;

public sealed class GetAccountsUseCase(
    IAccountRepository accounts,
    ICacheService cache) : IGetAccountsUseCase
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(15);

    public async Task<IReadOnlyList<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetAsync<List<AccountDto>>(CacheKeys.AccountsList, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var allAccounts = await accounts.GetAllAsync(cancellationToken);
        var result = allAccounts.Select(AccountDto.From).ToList();

        await cache.SetAsync(CacheKeys.AccountsList, result, CacheDuration, cancellationToken);

        return result;
    }
}
