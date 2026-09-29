using FinancialTransactions.Domain.Entities;

namespace FinancialTransactions.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<Account?> GetByIdWithLockAsync(Guid accountId, CancellationToken cancellationToken = default);
}
