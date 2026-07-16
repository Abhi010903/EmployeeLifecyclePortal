using Serilog.Context;

namespace EmployeeLifecyclePortal.Api.Middleware.Logging;

/// <summary>
/// Assigns or propagates a Correlation ID for every incoming HTTP request.
/// The ID is read from the <c>X-Correlation-Id</c> request header when present;
/// otherwise a new GUID is generated.  The value is:
/// <list type="bullet">
///   <item>Stored in <c>HttpContext.Items</c> so it can be resolved via <see cref="ICorrelationIdAccessor"/>.</item>
///   <item>Pushed into Serilog's <see cref="LogContext"/> so every log entry emitted
///         during the request is automatically enriched with the <c>CorrelationId</c> property.</item>
///   <item>Echoed back to the caller in the <c>X-Correlation-Id</c> response header.</item>
/// </list>
/// Must be registered as the first middleware in the pipeline.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request);

        // Make the ID available to other components (e.g. ICorrelationIdAccessor)
        context.Items[CorrelationIdHeader] = correlationId;

        // Echo the ID back so clients / load-balancers can trace the request
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Push the Correlation ID into Serilog's ambient log context so every
        // subsequent log statement emitted during this request is enriched with it.
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug(
                "CorrelationId {CorrelationId} assigned to {Method} {Path}",
                correlationId,
                context.Request.Method,
                context.Request.Path);

            await _next(context);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ResolveCorrelationId(HttpRequest request)
    {
        if (request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue!;
        }

        return Guid.NewGuid().ToString("N");
    }
}