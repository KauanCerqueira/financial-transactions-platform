using FinancialTransactions.Application.Abstractions.Caching;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.ValueObjects;

namespace FinancialTransactions.Application.Features.Accounts;

public sealed class CreateAccountUseCase(
    IAccountRepository accounts,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    TimeProvider timeProvider) : ICreateAccountUseCase
{
    public async Task<AccountDto> CreateAsync(
        CreateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        var account = Account.Open(
            Guid.NewGuid(),
            command.HolderName,
            Money.Create(command.InitialBalance),
            timeProvider.GetUtcNow());

        await accounts.AddAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // a lista de contas é cacheada: criar uma conta derruba a entrada
        await cache.RemoveAsync(CacheKeys.AccountsList, cancellationToken);

        return AccountDto.From(account);
    }
}
