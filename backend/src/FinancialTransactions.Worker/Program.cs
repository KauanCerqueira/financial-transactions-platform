using FinancialTransactions.Application;
using FinancialTransactions.Infrastructure;
using FinancialTransactions.Infrastructure.Messaging;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("service", "financial-transactions-worker")
    .Enrich.WithProperty("environment", builder.Environment.EnvironmentName));

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("A connection string 'Default' não foi configurada.");

var rabbitMqOptions = new RabbitMqOptions
{
    Uri = builder.Configuration["RabbitMq:Uri"] ?? string.Empty,
    QueueName = builder.Configuration["RabbitMq:QueueName"] ?? "transactions"
};

builder.Services.AddInfrastructure(
    connectionString,
    builder.Configuration.GetConnectionString("Redis"),
    rabbitMqOptions);
builder.Services.AddApplication();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<TransactionQueueConsumer>();

var host = builder.Build();

host.Run();
