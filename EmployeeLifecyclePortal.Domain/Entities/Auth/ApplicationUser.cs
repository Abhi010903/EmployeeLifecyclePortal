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
        string role,
        Guid? employeeId = null)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        EmployeeId = employeeId;
    }

    public string Username { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Role { get; private set; } = "User";

    public Guid? EmployeeId { get; private set; }

    public void LinkEmployee(Guid employeeId)
    {
        EmployeeId = employeeId;
    }

    public void UnlinkEmployee()
    {
        EmployeeId = null;
    }

    public void SetRole(string role)
    {
        Role = role;
    }
}