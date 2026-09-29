using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Application.Abstractions.Messaging;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace FinancialTransactions.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[EnableRateLimiting("transactions")]
public sealed class TransactionsController(
    IEnqueueTransactionUseCase enqueueTransaction,
    IGetTransactionEventUseCase getTransactionEvent,
    ITransactionQueue queue) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Enfileira um evento financeiro para processamento assíncrono",
        Description = "Recebe um crédito ou débito e enfileira para o worker atualizar o saldo. É idempotente pelo eventId: repetir o mesmo evento devolve o status já conhecido, sem duplicar.")]
    [SwaggerRequestExample(typeof(ProcessTransactionRequest), typeof(ProcessTransactionRequestExample))]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Evento recebido e enfileirado (PENDING).", typeof(TransactionAcceptedResponse))]
    [SwaggerResponse(StatusCodes.Status200OK, "Evento já enfileirado ou já processado.", typeof(TransactionAcceptedResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Payload inválido: tipo, valor ou data.")]
    [SwaggerResponse(StatusCodes.Status503ServiceUnavailable, "Fila de processamento indisponível.")]
    public async Task<ActionResult<TransactionAcceptedResponse>> Create(
        [FromBody] ProcessTransactionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TransactionTypeMapper.TryParse(request.Type, out var type))
        {
            ModelState.AddModelError(nameof(request.Type), "O tipo deve ser CREDIT ou DEBIT.");
            return ValidationProblem(ModelState);
        }

        if (!queue.IsEnabled)
        {
            return Problem(
                title: "Processamento indisponível.",
                detail: "A fila de transações não está configurada.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var command = new ProcessTransactionCommand(
            request.EventId,
            request.AccountId,
            type,
            request.Amount,
            request.OccurredAt);

        var result = await enqueueTransaction.EnqueueAsync(command, cancellationToken);
        var response = new TransactionAcceptedResponse(
            result.EventId,
            result.Status,
            result.RejectionCode,
            result.Transaction,
            result.AlreadyProcessed);

        return result.Status == TransactionEventStatus.Pending
            ? Accepted($"/api/transactions/{result.EventId}", response)
            : Ok(response);
    }

    [HttpGet("{eventId:guid}")]
    [SwaggerOperation(
        Summary = "Consulta o status de um evento",
        Description = "PENDING enquanto o worker não processar; PROCESSED com o lançamento, ou REJECTED com o motivo.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Status atual do evento.", typeof(TransactionAcceptedResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Evento desconhecido.")]
    public async Task<ActionResult<TransactionAcceptedResponse>> GetStatus(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var result = await getTransactionEvent.GetAsync(eventId, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(new TransactionAcceptedResponse(
            result.EventId,
            result.Status,
            result.RejectionCode,
            result.Transaction,
            result.AlreadyProcessed));
    }
}
