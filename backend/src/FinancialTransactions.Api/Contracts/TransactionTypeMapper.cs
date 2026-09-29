using FinancialTransactions.Domain.Enums;

namespace FinancialTransactions.Api.Contracts;

public static class TransactionTypeMapper
{
    public static bool TryParse(string? value, out TransactionType type)
    {
        switch (value?.Trim().ToUpperInvariant())
        {
            case "CREDIT":
                type = TransactionType.Credit;
                return true;
            case "DEBIT":
                type = TransactionType.Debit;
                return true;
            default:
                type = default;
                return false;
        }
    }
}
