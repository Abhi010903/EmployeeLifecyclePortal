using EmployeeLifecyclePortal.Domain.Common;
using EmployeeLifecyclePortal.Domain.Enums;

namespace EmployeeLifecyclePortal.Domain.Entities;

public class Employee : AuditableEntity
{
    private readonly List<EmployeeRole> _employeeRoles = [];

    public string EmployeeCode { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public EmploymentStatus Status { get; private set; }

    public Guid DepartmentId { get; private set; }

    public IReadOnlyCollection<EmployeeRole> EmployeeRoles
        => _employeeRoles.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}";

    private Employee()
    {
    }

    public Employee(
        string employeeCode,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        Guid departmentId)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new ArgumentException("Employee code is required.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");

        EmployeeCode = employeeCode;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        DepartmentId = departmentId;
        Status = EmploymentStatus.Active;
    }

    public void AssignDepartment(Guid departmentId)
    {
        DepartmentId = departmentId;
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public void AssignRole(Guid roleId)
    {
        if (_employeeRoles.Count >= 2)
            throw new InvalidOperationException(
                "An employee cannot have more than two roles.");

        bool roleAlreadyAssigned =
            _employeeRoles.Any(x => x.RoleId == roleId);

        if (roleAlreadyAssigned)
            return;

        _employeeRoles.Add(
            new EmployeeRole(Id, roleId));
    }

    public void Activate()
    {
        Status = EmploymentStatus.Active;
    }

    public void Deactivate()
    {
        Status = EmploymentStatus.Inactive;
    }

    public void Terminate()
    {
        Status = EmploymentStatus.Terminated;
    }
}