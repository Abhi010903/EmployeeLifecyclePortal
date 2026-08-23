using System.Security.Claims;

namespace EmployeeLifecyclePortal.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    string Email { get; }

    string Role { get; }

    bool IsAuthenticated { get; }

    ClaimsPrincipal User { get; }

    Guid? EmployeeId { get; }

    /// <summary>
    /// Gets the current user ID as a string for audit logging purposes.
    /// Returns null if no user is authenticated or user ID is not a valid GUID.
    /// </summary>
    string? GetCurrentUserId();

    /// <summary>
    /// Resolves the authenticated user's corresponding EmployeeId.
    /// Throws NotFoundException if no employee record is found.
    /// </summary>
    Task<Guid> GetRequiredEmployeeIdAsync(IApplicationDbContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the current user has permission to access or manage the specified employee.
    /// Admin/HR: full access.
    /// Manager/TeamLead: access to direct and indirect subordinates + self.
    /// Employee: access to self only.
    /// </summary>
    Task<bool> HasAccessToEmployeeAsync(Guid targetEmployeeId, IApplicationDbContext context, CancellationToken cancellationToken = default);
}