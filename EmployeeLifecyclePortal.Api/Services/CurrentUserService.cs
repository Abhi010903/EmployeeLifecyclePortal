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
}