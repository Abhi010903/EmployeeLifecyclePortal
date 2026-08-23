using EmployeeLifecyclePortal.Application.Exceptions;
using EmployeeLifecyclePortal.Application.Exceptions.Auth;
using System.Net;
using System.Text.Json;

namespace EmployeeLifecyclePortal.Api.Middleware;

/// <summary>
/// Global exception handler middleware.
/// Catches every unhandled exception that escapes the controller/handler pipeline,
/// maps it to the appropriate HTTP status code, logs it with structured properties
/// (including the ambient <c>CorrelationId</c> from Serilog's <c>LogContext</c>),
/// and returns a consistent JSON error body.
/// </summary>
public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiExceptionMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = MapToStatusCode(exception);
        var requestId = context.TraceIdentifier;

        LogException(exception, statusCode, context.Request, requestId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = BuildResponse(statusCode, exception);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    private void LogException(
        Exception exception,
        int statusCode,
        HttpRequest request,
        string requestId)
    {
        // 4xx errors are expected application conditions — log at Warning.
        // 5xx errors indicate unexpected failures — log at Error with full stack trace.
        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path} — Status: {StatusCode} | RequestId: {RequestId} | Message: {Message}",
                request.Method,
                request.Path,
                statusCode,
                requestId,
                exception.Message);
        }
        else
        {
            _logger.LogWarning(
                "Handled exception on {Method} {Path} — Status: {StatusCode} | RequestId: {RequestId} | Type: {ExceptionType} | Message: {Message}",
                request.Method,
                request.Path,
                statusCode,
                requestId,
                exception.GetType().Name,
                exception.Message);
        }
    }

    private static int MapToStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            NotFoundException => (int)HttpStatusCode.NotFound,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ForbiddenException => (int)HttpStatusCode.Forbidden,
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
            UserAlreadyExistsException => (int)HttpStatusCode.Conflict,
            InvalidCredentialsException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError
        };

    private static ExceptionResponse BuildResponse(int statusCode, Exception exception)
    {
        var message = exception.Message;

        // Sanitize messages containing internal GUIDs or domain IDs
        if (System.Text.RegularExpressions.Regex.IsMatch(message, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"))
        {
            if (statusCode == (int)HttpStatusCode.NotFound)
            {
                message = "The requested employee or resource could not be found. Please contact HR.";
            }
            else if (statusCode == (int)HttpStatusCode.Forbidden)
            {
                message = "You don't have permission to access this information.";
            }
        }

        var response = new ExceptionResponse
        {
            StatusCode = statusCode,
            Message = message
        };

        // Attach field-level validation errors when available
        if (exception is ValidationException validationException)
        {
            response.Errors = validationException.Errors;
        }

        return response;
    }
}
