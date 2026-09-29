namespace FinancialTransactions.Application.Exceptions;

public sealed class DuplicateEventException : Exception
{
    public DuplicateEventException(Exception innerException)
        : base("O evento informado já foi processado.", innerException)
    {
    }
}
