using Swashbuckle.AspNetCore.Filters;

namespace FinancialTransactions.Api.Contracts;

public sealed class ProcessTransactionRequestExample : IExamplesProvider<ProcessTransactionRequest>
{
    public ProcessTransactionRequest GetExamples() => new()
    {
        EventId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
        AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Type = "CREDIT",
        Amount = 150.75m,
        OccurredAt = new DateTimeOffset(2026, 1, 30, 10, 15, 0, TimeSpan.Zero)
    };
}
