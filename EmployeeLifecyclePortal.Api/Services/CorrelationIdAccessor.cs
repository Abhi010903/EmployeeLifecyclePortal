using EmployeeLifecyclePortal.Api.Middleware.Logging;
using EmployeeLifecyclePortal.Application.Interfaces.Logging;

namespace EmployeeLifecyclePortal.Api.Services;

/// <summary>
/// Resolves the Correlation ID for the current request from <see cref="IHttpContextAccessor"/>.
/// The value is placed into <c>HttpContext.Items</c> by <see cref="CorrelationIdMiddleware"/>.
/// </summary>
public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCorrelationId()
    {
        var items = _httpContextAccessor.HttpContext?.Items;

        if (items is null)
        {
            return null;
        }

        return items.TryGetValue(CorrelationIdMiddleware.CorrelationIdHeader, out var value)
            ? value?.ToString()
            : null;
    }
}
