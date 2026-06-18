namespace EmployeeLifecyclePortal.Application.DTOs.EmployeeRoles;

public sealed class AssignRoleDto
{
    public Guid EmployeeId { get; set; }

    public Guid RoleId { get; set; }
}