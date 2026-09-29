using System.Text.Json;
using System.Text.Json.Serialization;
using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Api.Errors;
using FinancialTransactions.Api.Health;
using FinancialTransactions.Api.Startup;
using FinancialTransactions.Application;
using FinancialTransactions.Infrastructure;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("A connection string 'Default' não foi configurada.");

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("service", "financial-transactions-api")
        .Enrich.WithProperty("environment", context.HostingEnvironment.EnvironmentName);

    var elasticsearchUri = context.Configuration["Observability:ElasticsearchUri"];

    if (!string.IsNullOrWhiteSpace(elasticsearchUri))
    {
        loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "financial-transactions-{0:yyyy.MM}"
        });
    }
});

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Financial Transactions API",
        Version = "v1",
        Description =
            "Processa transações financeiras e mantém o saldo consolidado das contas. " +
            "Garante idempotência por eventId, saldo nunca negativo e gravação transacional.",
        Contact = new OpenApiContact
        {
            Name = "Kauan Cerqueira",
            Url = new Uri("https://github.com/KauanCerqueira")
        }
    });

    options.EnableAnnotations();
    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<ProcessTransactionRequestExample>();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("postgres", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

await app.ApplyMigrationsAndSeedAsync();

app.Run();

public partial class Program;
