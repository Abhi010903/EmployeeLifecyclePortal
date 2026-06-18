using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

public class EmployeeRole : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    public Guid RoleId { get; private set; }

    private EmployeeRole()
    {
    }

    public EmployeeRole(
        Guid employeeId,
        Guid roleId)
    {
        EmployeeId = employeeId;
        RoleId = roleId;
    }
}