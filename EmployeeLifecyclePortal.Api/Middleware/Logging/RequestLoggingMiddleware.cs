using System.Diagnostics;

namespace EmployeeLifecyclePortal.Api.Middleware.Logging;

/// <summary>
/// Logs every HTTP request and its response at the API boundary, enriching each log
/// entry with structured properties that are already present in Serilog's
/// <c>LogContext</c> (e.g. <c>CorrelationId</c> pushed by <see cref="CorrelationIdMiddleware"/>).
///
/// What is captured per request:
/// <list type="bullet">
///   <item>HTTP method, path, query string, and client IP on arrival.</item>
///   <item>Response status code and elapsed milliseconds on completion.</item>
///   <item>A <c>Warning</c> when the request takes longer than the slow-request threshold (500 ms).</item>
///   <item>The <c>RequestId</c> from ASP.NET Core's <c>HttpContext.TraceIdentifier</c>.</item>
/// </list>
///
/// Must be placed after <see cref="CorrelationIdMiddleware"/> so the
/// <c>CorrelationId</c> LogContext property is already set.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    /// <summary>
    /// Requests that exceed this threshold are logged at Warning level so they
    /// stand out in log dashboards without requiring a separate performance monitor.
    /// </summary>
    private const long SlowRequestThresholdMs = 500;

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var requestId = context.TraceIdentifier;

        // Capture the full path including query string for structured logging
        var pathWithQuery = request.QueryString.HasValue
            ? $"{request.Path}{request.QueryString}"
            : request.Path.ToString();

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        _logger.LogInformation(
            "HTTP {Method} {Path} started — RequestId: {RequestId} | IP: {ClientIp}",
            request.Method,
            pathWithQuery,
            requestId,
            clientIp);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            if (elapsed >= SlowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "SLOW HTTP {Method} {Path} completed — Status: {StatusCode} | Elapsed: {ElapsedMs} ms | RequestId: {RequestId} | IP: {ClientIp}",
                    request.Method,
                    pathWithQuery,
                    statusCode,
                    elapsed,
                    requestId,
                    clientIp);
            }
            else
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} completed — Status: {StatusCode} | Elapsed: {ElapsedMs} ms | RequestId: {RequestId}",
                    request.Method,
                    pathWithQuery,
                    statusCode,
                    elapsed,
                    requestId);
            }
        }
    }
}
