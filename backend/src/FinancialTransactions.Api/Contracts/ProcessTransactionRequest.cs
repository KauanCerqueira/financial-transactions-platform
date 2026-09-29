using System.ComponentModel.DataAnnotations;

namespace FinancialTransactions.Api.Contracts;

public sealed class ProcessTransactionRequest
{
    private const double MinimumAmount = 0.01;
    private const double MaximumAmount = 1_000_000_000;

    public Guid EventId { get; init; }

    public Guid AccountId { get; init; }

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    public string Type { get; init; } = string.Empty;

    [Range(MinimumAmount, MaximumAmount, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; init; }

    public DateTimeOffset OccurredAt { get; init; }
}
