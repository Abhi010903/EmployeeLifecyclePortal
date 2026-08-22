namespace EmployeeLifecyclePortal.Application.DTOs.Employees;

public sealed class EmployeeDto
{
    public Guid Id { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Status { get; set; } = "Active";

    public Guid DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public Guid? ManagerId { get; set; }

    public string? ManagerName { get; set; }

    public Guid? TeamLeadId { get; set; }

    public string? TeamLeadName { get; set; }

    public Guid? RoleId { get; set; }

    public string? RoleName { get; set; }

    public List<string> Roles { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}