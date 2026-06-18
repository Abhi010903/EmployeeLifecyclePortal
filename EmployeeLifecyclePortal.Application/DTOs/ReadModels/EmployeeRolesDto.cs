namespace EmployeeLifecyclePortal.Application.DTOs.ReadModels;

public sealed class EmployeeRolesDto
{
    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}