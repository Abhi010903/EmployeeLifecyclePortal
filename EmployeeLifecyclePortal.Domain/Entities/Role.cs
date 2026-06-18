using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

public class Role : AuditableEntity
{
    private readonly List<EmployeeRole> _employeeRoles = [];

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<EmployeeRole> EmployeeRoles
        => _employeeRoles.AsReadOnly();

    private Role()
    {
    }

    public Role(
        string name,
        string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.");

        Name = name;
        Description = description;
    }

    public void UpdateDescription(
        string description)
    {
        Description = description;
    }

    public void AssignToEmployee(
        Guid employeeId)
    {
        bool alreadyAssigned =
            _employeeRoles.Any(x => x.EmployeeId == employeeId);

        if (alreadyAssigned)
            return;

        _employeeRoles.Add(
            new EmployeeRole(employeeId, Id));
    }
}