using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Features.Transactions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FinancialTransactions.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController(IProcessTransactionUseCase processTransaction) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Processa um evento financeiro",
        Description = "Recebe um crédito ou débito. É idempotente pelo eventId: repetir o mesmo evento devolve o lançamento já processado, sem movimentar o saldo de novo.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Evento processado e saldo atualizado.", typeof(TransactionDto))]
    [SwaggerResponse(StatusCodes.Status200OK, "Evento já processado anteriormente (idempotência).", typeof(TransactionDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Payload inválido: tipo, valor ou data.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Conta não encontrada.")]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada, como saldo insuficiente.")]
    public async Task<ActionResult<TransactionDto>> Create(
        [FromBody] ProcessTransactionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TransactionTypeMapper.TryParse(request.Type, out var type))
        {
            ModelState.AddModelError(nameof(request.Type), "O tipo deve ser CREDIT ou DEBIT.");
            return ValidationProblem(ModelState);
        }

        var command = new ProcessTransactionCommand(
            request.EventId,
            request.AccountId,
            type,
            request.Amount,
            request.OccurredAt);

        var result = await processTransaction.ProcessAsync(command, cancellationToken);

        return result.AlreadyProcessed
            ? Ok(result.Transaction)
            : StatusCode(StatusCodes.Status201Created, result.Transaction);
    }
}
