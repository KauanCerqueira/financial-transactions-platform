using System.Text.Json;
using System.Text.Json.Serialization;
using FinancialTransactions.Api.Contracts;
using FinancialTransactions.Api.Errors;
using FinancialTransactions.Api.Health;
using FinancialTransactions.Api.Startup;
using FinancialTransactions.Application;
using FinancialTransactions.Infrastructure;
using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.AddInfrastructure(
    connectionString,
    builder.Configuration.GetConnectionString("Redis"),
    new RabbitMqOptions
    {
        Uri = builder.Configuration["RabbitMq:Uri"] ?? string.Empty,
        QueueName = builder.Configuration["RabbitMq:QueueName"] ?? "transactions"
    });
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

var authenticationAuthority = builder.Configuration["Authentication:Authority"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authenticationAuthority;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = builder.Configuration["Authentication:Audience"],
            ValidIssuers = builder.Configuration.GetSection("Authentication:Issuers").Get<string[]>()
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Protege todas as rotas quando o Keycloak está configurado; sem ele, o ambiente fica aberto (dev/testes).
    if (!string.IsNullOrWhiteSpace(authenticationAuthority))
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    }
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("transactions", limiter =>
    {
        limiter.PermitLimit = 30;
        limiter.Window = TimeSpan.FromSeconds(10);
        limiter.QueueLimit = 0;
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Muitas requisições.",
            Detail = "Aguarde alguns segundos e tente novamente.",
            Instance = context.HttpContext.Request.Path
        };

        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    };
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();

await app.ApplyMigrationsAndSeedAsync();

app.Run();

public partial class Program;
