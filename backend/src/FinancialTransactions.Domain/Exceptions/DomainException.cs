namespace FinancialTransactions.Domain.Exceptions;

// erro de regra de negócio (esperado) -> vira 4xx; falha técnica -> 500
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
