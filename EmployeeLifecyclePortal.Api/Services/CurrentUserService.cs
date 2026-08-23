using EmployeeLifecyclePortal.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EmployeeLifecyclePortal.Api.Services;

public sealed class CurrentUserService
    : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal UserPrincipal =>
        _httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal();

    public ClaimsPrincipal User => UserPrincipal;

    public bool IsAuthenticated =>
        UserPrincipal.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var claim = UserPrincipal.FindFirst(
                ClaimTypes.NameIdentifier);

            return Guid.TryParse(
                claim?.Value,
                out var id)
                ? id
                : Guid.Empty;
        }
    }

    public string Email =>
        UserPrincipal.FindFirst(
            ClaimTypes.Email)?.Value
        ?? string.Empty;

    public string Role =>
        UserPrincipal.FindFirst(
            ClaimTypes.Role)?.Value
        ?? string.Empty;

    public Guid? EmployeeId
    {
        get
        {
            var claim = UserPrincipal.FindFirst("employee_id")
                ?? UserPrincipal.FindFirst("EmployeeId");

            return Guid.TryParse(claim?.Value, out var id) && id != Guid.Empty
                ? id
                : null;
        }
    }

    public string? GetCurrentUserId()
    {
        if (!IsAuthenticated)
        {
            return null;
        }

        var userIdGuid = UserId;
        return userIdGuid == Guid.Empty ? null : userIdGuid.ToString();
    }

    public async Task<Guid> GetRequiredEmployeeIdAsync(
        IApplicationDbContext context,
        CancellationToken cancellationToken = default)
    {
        if (EmployeeId.HasValue && EmployeeId.Value != Guid.Empty)
        {
            return EmployeeId.Value;
        }

        if (UserId != Guid.Empty)
        {
            var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(context.ApplicationUsers, u => u.Id == UserId, cancellationToken);

            if (user?.EmployeeId.HasValue == true && user.EmployeeId.Value != Guid.Empty)
            {
                return user.EmployeeId.Value;
            }

            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var emp = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(context.Employees, e => e.Email.ToLower() == user.Email.ToLower(), cancellationToken);

                if (emp != null)
                {
                    user.LinkEmployee(emp.Id);
                    await context.SaveChangesAsync(cancellationToken);
                    return emp.Id;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(Email))
        {
            var emp = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(context.Employees, e => e.Email.ToLower() == Email.ToLower(), cancellationToken);

            if (emp != null)
            {
                return emp.Id;
            }
        }

        throw new EmployeeLifecyclePortal.Application.Exceptions.NotFoundException(
            "Your employee profile could not be found. Please contact HR.");
    }

    public async Task<bool> HasAccessToEmployeeAsync(
        Guid targetEmployeeId,
        IApplicationDbContext context,
        CancellationToken cancellationToken = default)
    {
        if (targetEmployeeId == Guid.Empty)
        {
            return false;
        }

        if (Role == "Admin" || Role == "HR")
        {
            return true;
        }

        Guid currentEmpId;
        try
        {
            currentEmpId = await GetRequiredEmployeeIdAsync(context, cancellationToken);
        }
        catch
        {
            return false;
        }

        if (targetEmployeeId == currentEmpId)
        {
            return true;
        }

        if (Role == "Manager" || Role == "Team Lead" || Role == "TeamLead" || Role == "Supervisor")
        {
            // Check direct or indirect reporting
            var isSubordinate = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .AnyAsync(context.Employees, e => e.Id == targetEmployeeId && (e.ManagerId == currentEmpId || e.TeamLeadId == currentEmpId), cancellationToken);

            if (isSubordinate)
            {
                return true;
            }
        }

        return false;
    }
}