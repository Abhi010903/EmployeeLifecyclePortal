using System.Security.Claims;

namespace EmployeeLifecyclePortal.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    string Email { get; }

    string Role { get; }

    bool IsAuthenticated { get; }

    ClaimsPrincipal User { get; }
}