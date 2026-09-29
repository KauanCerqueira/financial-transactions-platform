using FinancialTransactions.Application.Abstractions.Caching;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Exceptions;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed class GetStatementUseCase(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    ICacheService cache) : IGetStatementUseCase
{
    private const int MaxPageSize = 100;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<PagedResult<TransactionDto>> GetStatementAsync(
        GetStatementQuery query,
        CancellationToken cancellationToken = default)
    {
        await EnsureAccountExistsAsync(query.AccountId, cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var cacheKey = await BuildCacheKeyAsync(query.AccountId, page, pageSize, cancellationToken);
        var cached = await cache.GetAsync<PagedResult<TransactionDto>>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var skip = (page - 1) * pageSize;
        var pageItems = await transactions.GetPageAsync(query.AccountId, skip, pageSize, cancellationToken);
        var totalItems = await transactions.CountByAccountIdAsync(query.AccountId, cancellationToken);

        var result = new PagedResult<TransactionDto>(
            pageItems.Select(TransactionDto.From).ToList(),
            page,
            pageSize,
            totalItems);

        await cache.SetAsync(cacheKey, result, CacheDuration, cancellationToken);

        return result;
    }

    private async Task<string> BuildCacheKeyAsync(
        Guid accountId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var version = await cache.GetAsync<string>(CacheKeys.StatementVersion(accountId), cancellationToken) ?? "0";

        return $"statement:{accountId}:{version}:{page}:{pageSize}";
    }

    private async Task EnsureAccountExistsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await accounts.GetByIdAsync(accountId, cancellationToken);

        if (account is null)
        {
            throw new AccountNotFoundException(accountId);
        }
    }
}
