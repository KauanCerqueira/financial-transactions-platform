namespace FinancialTransactions.Application.Exceptions;

public sealed class DuplicateEventException : Exception
{
    public DuplicateEventException(Exception innerException)
        : base("O evento já foi processado (violação de unicidade).", innerException)
    {
    }
}
