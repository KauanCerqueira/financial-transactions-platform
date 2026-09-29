using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Application.Features.Accounts;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FinancialTransactions.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(
    IGetAccountsUseCase getAccounts,
    IGetAccountsSummaryUseCase getAccountsSummary,
    ICreateAccountUseCase createAccount,
    IGetStatementUseCase getStatement) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Lista as contas com o saldo consolidado")]
    [SwaggerResponse(StatusCodes.Status200OK, "Contas retornadas com sucesso.", typeof(IReadOnlyList<AccountDto>))]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await getAccounts.GetAllAsync(cancellationToken));

    [HttpPost]
    [SwaggerOperation(
        Summary = "Cria uma conta",
        Description = "Cadastra uma nova conta. Saldo inicial maior que zero entra como lançamento de abertura no extrato, para saldo e histórico nunca divergirem.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Conta criada com sucesso.", typeof(AccountDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos: titular vazio ou saldo inicial negativo.")]
    public async Task<ActionResult<AccountDto>> Create(
        CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var account = await createAccount.CreateAsync(
            new CreateAccountCommand(request.HolderName, request.InitialBalance),
            cancellationToken);

        return Created($"/api/accounts/{account.Id}/transactions", account);
    }

    [HttpGet("summary")]
    [SwaggerOperation(
        Summary = "Resumo das contas",
        Description = "Quantidade de contas, saldo consolidado e total de lançamentos registrados.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Resumo retornado com sucesso.", typeof(AccountsSummaryDto))]
    public async Task<ActionResult<AccountsSummaryDto>> GetSummary(CancellationToken cancellationToken) =>
        Ok(await getAccountsSummary.ExecuteAsync(cancellationToken));

    [HttpGet("{accountId:guid}/transactions")]
    [SwaggerOperation(
        Summary = "Retorna o extrato paginado de uma conta",
        Description = "Cada lançamento traz o saldo após a movimentação (balanceAfter). Aceita filtros por tipo (CREDIT/DEBIT) e período. A página começa em 1 e o tamanho fica entre 1 e 100. A resposta inclui o resumo de créditos e débitos do filtro inteiro.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Extrato retornado com sucesso.", typeof(StatementDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Filtro de tipo inválido.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Conta não encontrada.")]
    public async Task<ActionResult<StatementDto>> GetTransactions(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? type = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        TransactionType? transactionType = null;

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!TransactionTypeMapper.TryParse(type, out var parsed))
            {
                ModelState.AddModelError(nameof(type), "O tipo deve ser CREDIT ou DEBIT.");
                return ValidationProblem(ModelState);
            }

            transactionType = parsed;
        }

        var query = new GetStatementQuery(accountId, page, pageSize, transactionType, from, to);

        return Ok(await getStatement.GetStatementAsync(query, cancellationToken));
    }
}
