using System.Diagnostics;

namespace EmployeeLifecyclePortal.Api.Middleware.Logging;

public sealed class StructuredLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StructuredLoggingMiddleware> _logger;

    public StructuredLoggingMiddleware(RequestDelegate next, ILogger<StructuredLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var correlationId = context.Request.Headers.ContainsKey("X-Correlation-Id")
            ? context.Request.Headers["X-Correlation-Id"].ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Items["TraceId"] = traceId;

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                _logger.LogInformation(
                    "Request started: Method={Method}, Path={Path}, CorrelationId={CorrelationId}, TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId,
                    traceId);

                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Request completed: Method={Method}, Path={Path}, StatusCode={StatusCode}, Duration={DurationMs}ms, CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);

                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Request failed: Method={Method}, Path={Path}, Duration={DurationMs}ms, CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

public static class StructuredLoggingExtensions
{
    public static IApplicationBuilder UseStructuredLogging(this IApplicationBuilder builder)
        => builder.UseMiddleware<StructuredLoggingMiddleware>();
}
