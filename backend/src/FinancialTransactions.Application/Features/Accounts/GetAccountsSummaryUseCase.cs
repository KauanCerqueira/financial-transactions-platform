using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Accounts;

public sealed class GetAccountsSummaryUseCase(
    IAccountRepository accounts,
    ITransactionRepository transactions) : IGetAccountsSummaryUseCase
{
    public async Task<AccountsSummaryDto> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var allAccounts = await accounts.GetAllAsync(cancellationToken);
        var transactionCount = await transactions.CountAsync(cancellationToken);
        var totalBalance = allAccounts.Sum(account => account.Balance.Amount);

        return new AccountsSummaryDto(allAccounts.Count, totalBalance, transactionCount);
    }
}
