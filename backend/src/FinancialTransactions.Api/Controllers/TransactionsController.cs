using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Features.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController(IProcessTransactionUseCase processTransaction) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
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
