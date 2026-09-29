using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Features.Accounts;
using FinancialTransactions.Application.Features.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(
    IGetAccountsUseCase getAccounts,
    IGetStatementUseCase getStatement) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AccountDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await getAccounts.ExecuteAsync(cancellationToken));

    [HttpGet("{accountId:guid}/transactions")]
    [ProducesResponseType<PagedResult<TransactionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetStatement(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await getStatement.ExecuteAsync(new GetStatementQuery(accountId, page, pageSize), cancellationToken));
}
