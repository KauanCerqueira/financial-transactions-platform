using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Accounts;

public interface IGetAccountsSummaryUseCase
{
    Task<AccountsSummaryDto> ExecuteAsync(CancellationToken cancellationToken = default);
}
