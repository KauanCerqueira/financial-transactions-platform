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
    IProcessTransactionUseCase processTransaction,
    IEnqueueTransactionUseCase enqueueTransaction,
    IGetTransactionEventUseCase getTransactionEvent,
    ITransactionQueue queue) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Processa um evento financeiro (crédito ou débito)",
        Description = "Atualiza o saldo na hora. É idempotente pelo eventId: repetir o mesmo evento devolve o lançamento já processado, sem movimentar o saldo de novo.")]
    [SwaggerRequestExample(typeof(ProcessTransactionRequest), typeof(ProcessTransactionRequestExample))]
    [SwaggerResponse(StatusCodes.Status201Created, "Evento processado e saldo atualizado.", typeof(TransactionAcceptedResponse))]
    [SwaggerResponse(StatusCodes.Status200OK, "Evento já processado anteriormente (idempotência).", typeof(TransactionAcceptedResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Payload inválido: tipo, valor ou data.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Conta não encontrada.")]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada, como saldo insuficiente.")]
    public async Task<ActionResult<TransactionAcceptedResponse>> Create(
        [FromBody] ProcessTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var command = BuildCommand(request);

        if (command is null)
        {
            return ValidationProblem(ModelState);
        }

        var result = await processTransaction.ProcessAsync(command, cancellationToken);
        var response = new TransactionAcceptedResponse(
            result.Transaction.EventId,
            TransactionEventStatus.Processed,
            null,
            result.Transaction,
            result.AlreadyProcessed);

        return result.AlreadyProcessed
            ? Ok(response)
            : StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("async")]
    [SwaggerOperation(
        Summary = "Enfileira um evento para processamento assíncrono (RabbitMQ)",
        Description = "Diferencial: o evento é gravado como pendente e publicado na fila; o worker atualiza o saldo depois. Consulte o resultado em GET /api/transactions/{eventId}.")]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Evento recebido e enfileirado (PENDING).", typeof(TransactionAcceptedResponse))]
    [SwaggerResponse(StatusCodes.Status200OK, "Evento já enfileirado ou já processado.", typeof(TransactionAcceptedResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Payload inválido: tipo, valor ou data.")]
    [SwaggerResponse(StatusCodes.Status503ServiceUnavailable, "Fila de processamento indisponível.")]
    public async Task<ActionResult<TransactionAcceptedResponse>> CreateAsync(
        [FromBody] ProcessTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var command = BuildCommand(request);

        if (command is null)
        {
            return ValidationProblem(ModelState);
        }

        if (!queue.IsEnabled)
        {
            return Problem(
                title: "Processamento indisponível.",
                detail: "A fila de transações não está configurada.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

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
        Summary = "Consulta o status de um evento enfileirado",
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

    private ProcessTransactionCommand? BuildCommand(ProcessTransactionRequest request)
    {
        if (!TransactionTypeMapper.TryParse(request.Type, out var type))
        {
            ModelState.AddModelError(nameof(request.Type), "O tipo deve ser CREDIT ou DEBIT.");
            return null;
        }

        return new ProcessTransactionCommand(
            request.EventId,
            request.AccountId,
            type,
            request.Amount,
            request.OccurredAt);
    }
}
