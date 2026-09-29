namespace FinancialTransactions.Application.Dtos;

public sealed record StatementDto(PagedResult<TransactionDto> Page, StatementSummaryDto Summary);
