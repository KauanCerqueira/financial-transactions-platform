namespace FinancialTransactions.Application.Abstractions.Caching;

public static class CacheKeys
{
    public const string AccountsList = "accounts:list";

    public static string StatementVersion(Guid accountId) => $"statement-version:{accountId}";
}
