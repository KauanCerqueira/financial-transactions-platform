using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Transactions;

public interface IGetStatementUseCase
{
    Task<StatementDto> GetStatementAsync(
        GetStatementQuery query,
        CancellationToken cancellationToken = default);
}
