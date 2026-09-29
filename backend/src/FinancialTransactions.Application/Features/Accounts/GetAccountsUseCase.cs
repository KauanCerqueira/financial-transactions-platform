using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Accounts;

public sealed class GetAccountsUseCase(IAccountRepository accounts) : IGetAccountsUseCase
{
    public async Task<IReadOnlyList<AccountDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var allAccounts = await accounts.GetAllAsync(cancellationToken);

        return allAccounts.Select(AccountDto.From).ToList();
    }
}
