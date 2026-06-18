namespace EmployeeLifecyclePortal.Application.DTOs.ReadModels;

public sealed class DepartmentEmployeesDto
{
    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public List<string> Employees { get; set; } = [];
}