using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Transactions;

public sealed class GetStatementUseCase(ITransactionRepository transactions) : IGetStatementUseCase
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<TransactionDto>> GetStatementAsync(
        GetStatementQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var skip = (page - 1) * pageSize;

        var pageItems = await transactions.GetPageAsync(query.AccountId, skip, pageSize, cancellationToken);
        var totalItems = await transactions.CountByAccountIdAsync(query.AccountId, cancellationToken);

        return new PagedResult<TransactionDto>(
            pageItems.Select(TransactionDto.From).ToList(),
            page,
            pageSize,
            totalItems);
    }
}
