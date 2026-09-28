namespace FinancialTransactions.Domain.Enums;

/// <summary>
/// Direção de um lançamento financeiro.
/// Crédito aumenta o saldo; Débito diminui.
/// </summary>
public enum TransactionType
{
    Credit = 1,
    Debit = 2
}
