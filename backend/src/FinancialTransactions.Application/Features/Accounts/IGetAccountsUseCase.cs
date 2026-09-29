using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Accounts;

public interface IGetAccountsUseCase
{
    Task<IReadOnlyList<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
