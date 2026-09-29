using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Transactions;

public interface IGetStatementUseCase
{
    Task<PagedResult<TransactionDto>> GetStatementAsync(
        GetStatementQuery query,
        CancellationToken cancellationToken = default);
}
