namespace FinancialTransactions.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public string Uri { get; init; } = string.Empty;

    public string QueueName { get; init; } = "transactions";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Uri);
}
