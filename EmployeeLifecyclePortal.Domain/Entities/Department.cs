using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

public class Department : AuditableEntity
{
    private readonly List<Employee> _employees = [];

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<Employee> Employees
        => _employees.AsReadOnly();

    private Department()
    {
    }

    public Department(
        string name,
        string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name is required.");

        Name = name;
        Description = description;
    }

    public void UpdateDescription(
        string description)
    {
        Description = description;
    }

    public void AddEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        bool employeeAlreadyExists =
            _employees.Any(x => x.Id == employee.Id);

        if (employeeAlreadyExists)
            return;

        _employees.Add(employee);
    }
}