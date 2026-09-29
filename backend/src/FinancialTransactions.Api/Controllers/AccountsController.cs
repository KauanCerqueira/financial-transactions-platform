using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Features.Accounts;
using FinancialTransactions.Application.Features.Transactions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FinancialTransactions.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(
    IGetAccountsUseCase getAccounts,
    IGetStatementUseCase getStatement) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Lista as contas com o saldo consolidado")]
    [SwaggerResponse(StatusCodes.Status200OK, "Contas retornadas com sucesso.", typeof(IReadOnlyList<AccountDto>))]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await getAccounts.GetAllAsync(cancellationToken));

    [HttpGet("{accountId:guid}/transactions")]
    [SwaggerOperation(
        Summary = "Retorna o extrato paginado de uma conta",
        Description = "Cada lançamento traz o saldo após a movimentação (balanceAfter). A página começa em 1 e o tamanho fica entre 1 e 100.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Extrato retornado com sucesso.", typeof(PagedResult<TransactionDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Conta não encontrada.")]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await getStatement.GetStatementAsync(new GetStatementQuery(accountId, page, pageSize), cancellationToken));
}
