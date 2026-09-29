using System.ComponentModel.DataAnnotations;

namespace FinancialTransactions.Api.Contracts;

public sealed class ProcessTransactionRequest
{
    public Guid EventId { get; init; }

    public Guid AccountId { get; init; }

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    public string Type { get; init; } = string.Empty;

    [Range(0.01, 1000000000.0, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; init; }

    public DateTimeOffset OccurredAt { get; init; }
}
