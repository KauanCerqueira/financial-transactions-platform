using FinancialTransactions.Application.Exceptions;
using FinancialTransactions.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Errors;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const int InternalServerError = StatusCodes.Status500InternalServerError;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var error = Map(exception);

        if (error.StatusCode >= InternalServerError)
        {
            logger.LogError(
                exception,
                "Erro não tratado em {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.StatusCode >= InternalServerError ? "Ocorreu um erro inesperado." : exception.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["code"] = error.Code;

        httpContext.Response.StatusCode = error.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ApiError Map(Exception exception) => exception switch
    {
        InsufficientFundsException => new ApiError(
            StatusCodes.Status422UnprocessableEntity, ApiErrorCodes.InsufficientFunds, "Saldo insuficiente."),
        AccountNotFoundException => new ApiError(
            StatusCodes.Status404NotFound, ApiErrorCodes.AccountNotFound, "Conta não encontrada."),
        DuplicateEventException => new ApiError(
            StatusCodes.Status409Conflict, ApiErrorCodes.DuplicateEvent, "Evento duplicado."),
        DomainException => new ApiError(
            StatusCodes.Status422UnprocessableEntity, ApiErrorCodes.Domain, "Regra de negócio violada."),
        _ => new ApiError(InternalServerError, ApiErrorCodes.Unexpected, "Erro interno.")
    };

    private readonly record struct ApiError(int StatusCode, string Code, string Title);
}
