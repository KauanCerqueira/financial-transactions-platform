using FinancialTransactions.Application.Dtos;

namespace FinancialTransactions.Application.Features.Accounts;

public interface ICreateAccountUseCase
{
    Task<AccountDto> CreateAsync(
        CreateAccountCommand command,
        CancellationToken cancellationToken = default);
}
