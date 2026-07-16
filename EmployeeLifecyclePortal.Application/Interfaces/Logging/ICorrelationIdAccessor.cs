namespace EmployeeLifecyclePortal.Application.Interfaces.Logging;

/// <summary>
/// Provides access to the current request's Correlation ID
/// so application-layer handlers can include it in structured log output.
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Returns the Correlation ID that was assigned to (or received from) the current
    /// HTTP request.  Returns <c>null</c> when called outside an active request context.
    /// </summary>
    string? GetCorrelationId();
}
