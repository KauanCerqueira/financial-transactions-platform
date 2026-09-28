namespace FinancialTransactions.Domain.Exceptions;

/// <summary>
/// Exceção base para TODAS as violações de regra de negócio do domínio.
/// Serve para diferenciar "o cliente violou uma regra de negócio" (esperado)
/// de "o sistema quebrou" (bug, banco fora do ar...), que deve virar erro 500.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
