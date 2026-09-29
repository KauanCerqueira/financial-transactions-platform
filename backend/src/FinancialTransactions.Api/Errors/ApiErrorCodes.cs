namespace FinancialTransactions.Api.Errors;

public static class ApiErrorCodes
{
    public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
    public const string DuplicateEvent = "DUPLICATE_EVENT";
    public const string AccountNotFound = "ACCOUNT_NOT_FOUND";
    public const string Domain = "DOMAIN_ERROR";
    public const string Unexpected = "UNEXPECTED_ERROR";
}
