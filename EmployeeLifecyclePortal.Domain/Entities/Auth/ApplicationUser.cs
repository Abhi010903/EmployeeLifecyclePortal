using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities.Auth;

public sealed class ApplicationUser : AuditableEntity
{
    private ApplicationUser()
    {
    }

    public ApplicationUser(
        string username,
        string email,
        string passwordHash,
        string role)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public string Username { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Role { get; private set; } = "User";
}