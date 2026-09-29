using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace FinancialTransactions.Api.Contracts;

public sealed class CreateAccountRequest
{
    private const int MaximumHolderNameLength = 200;
    private const double MaximumInitialBalance = 1_000_000_000;

    [Required(ErrorMessage = "O titular é obrigatório.")]
    [MaxLength(MaximumHolderNameLength, ErrorMessage = "O titular deve ter no máximo 200 caracteres.")]
    [SwaggerSchema(Description = "Nome do titular da conta. Ex.: Maria Silva")]
    public string HolderName { get; init; } = string.Empty;

    [Range(0, MaximumInitialBalance, ErrorMessage = "O saldo inicial não pode ser negativo.")]
    [SwaggerSchema(
        Description = "Saldo inicial em reais, opcional. Maior que zero entra como lançamento de abertura. Ex.: 1500.00")]
    public decimal InitialBalance { get; init; }
}
