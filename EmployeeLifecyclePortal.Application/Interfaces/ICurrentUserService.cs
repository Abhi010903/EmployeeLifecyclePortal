using System.Security.Claims;

namespace EmployeeLifecyclePortal.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    string Email { get; }

    string Role { get; }

    bool IsAuthenticated { get; }

    ClaimsPrincipal User { get; }

    /// <summary>
    /// Gets the current user ID as a string for audit logging purposes.
    /// Returns null if no user is authenticated or user ID is not a valid GUID.
    /// </summary>
    string? GetCurrentUserId();
}