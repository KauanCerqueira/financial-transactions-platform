using System.Text.Json;
using System.Text.Json.Serialization;
using FinancialTransactions.Api.Errors;
using FinancialTransactions.Api.Startup;
using FinancialTransactions.Application;
using FinancialTransactions.Infrastructure;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("A connection string 'Default' não foi configurada.");

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
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

await app.ApplyMigrationsAndSeedAsync();

app.Run();

public partial class Program;
