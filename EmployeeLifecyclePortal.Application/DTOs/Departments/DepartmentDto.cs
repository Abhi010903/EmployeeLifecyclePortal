namespace EmployeeLifecyclePortal.Application.DTOs.Departments;

public sealed class DepartmentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}