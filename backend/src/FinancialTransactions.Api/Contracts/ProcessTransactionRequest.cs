using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace FinancialTransactions.Api.Contracts;

public sealed class ProcessTransactionRequest
{
    private const double MinimumAmount = 0.01;
    private const double MaximumAmount = 1_000_000_000;

    [SwaggerSchema(Description = "Identificador único do evento. Repetir o mesmo valor não duplica o lançamento.")]
    public Guid EventId { get; init; }

    [SwaggerSchema(Description = "Conta que será movimentada.")]
    public Guid AccountId { get; init; }

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    [SwaggerSchema(Description = "CREDIT aumenta o saldo; DEBIT diminui.")]
    public string Type { get; init; } = string.Empty;

    [Range(MinimumAmount, MaximumAmount, ErrorMessage = "O valor deve ser maior que zero.")]
    [SwaggerSchema(Description = "Valor em reais, maior que zero. Ex.: 150.75")]
    public decimal Amount { get; init; }

    [SwaggerSchema(Description = "Data e hora do evento em ISO 8601 (UTC). Ex.: 2026-01-30T10:15:00Z")]
    public DateTimeOffset OccurredAt { get; init; }
}
