using FinancialTransactions.Domain.Entities;

namespace FinancialTransactions.Application.Dtos;

public sealed record AccountDto(
    Guid Id,
    string HolderName,
    decimal Balance,
    DateTimeOffset CreatedAt)
{
    public static AccountDto From(Account account) =>
        new(account.Id, account.HolderName, account.Balance.Amount, account.CreatedAt);
}
