using System.Runtime.ExceptionServices;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Api.Middleware;

public sealed partial class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            LogClientCancelledRequest(logger, GetTraceId(context));
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            LogUnhandledExceptionAfterResponseStarted(logger, exception, GetTraceId(context));
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        var problemDetails = CreateProblemDetails(context, exception);

        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
        {
            LogUnhandledException(logger, exception, GetTraceId(context));
        }
        else
        {
            LogHandledApplicationException(logger, exception, GetTraceId(context));
        }

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails, context.RequestAborted);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ApplicationValidationException validationException => CreateValidationProblemDetails(context, validationException),
            ApplicationUnauthorizedException => CreateProblemDetails(context, StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            ApplicationForbiddenException => CreateProblemDetails(context, StatusCodes.Status403Forbidden, "Forbidden", exception.Message),
            ApplicationNotFoundException => CreateProblemDetails(context, StatusCodes.Status404NotFound, "Not Found", exception.Message),
            ApplicationConflictException => CreateProblemDetails(context, StatusCodes.Status409Conflict, "Conflict", exception.Message),
            DomainException => CreateProblemDetails(context, StatusCodes.Status400BadRequest, "Domain rule violation", exception.Message),
            _ => CreateProblemDetails(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.")
        };

        problemDetails.Extensions["traceId"] = GetTraceId(context);

        return problemDetails;
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ApplicationValidationException exception)
    {
        var errors = exception.Errors.ToDictionary(
            entry => entry.Key,
            entry => entry.Value,
            StringComparer.Ordinal);

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed.",
            Detail = exception.Message,
            Type = "https://httpstatuses.com/400",
            Instance = context.Request.Path
        };
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = context.Request.Path
        };
    }

    private static string GetTraceId(HttpContext context)
    {
        return Activity.Current?.Id ?? context.TraceIdentifier;
    }

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Request was cancelled by the client. TraceId: {TraceId}.")]
    private static partial void LogClientCancelledRequest(ILogger logger, string traceId);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Error,
        Message = "Unhandled exception after response started. TraceId: {TraceId}.")]
    private static partial void LogUnhandledExceptionAfterResponseStarted(ILogger logger, Exception exception, string traceId);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Error,
        Message = "Unhandled exception. TraceId: {TraceId}.")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string traceId);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Warning,
        Message = "Handled application exception. TraceId: {TraceId}.")]
    private static partial void LogHandledApplicationException(ILogger logger, Exception exception, string traceId);
}
