using EmployeeLifecyclePortal.Domain.Common;
using EmployeeLifecyclePortal.Domain.Enums;

namespace EmployeeLifecyclePortal.Domain.Entities;

public class Employee : AuditableEntity
{
    private readonly List<EmployeeRole> _employeeRoles = [];
    private readonly List<EmployeeTimeline> _timelines = [];
    private readonly List<EmployeeDocument> _documents = [];

    public string EmployeeCode { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public EmploymentStatus Status { get; private set; }

    public Guid DepartmentId { get; private set; }

    /// <summary>
    /// The manager's ID (employee who supervises this employee)
    /// </summary>
    public Guid? ManagerId { get; private set; }

    /// <summary>
    /// Navigation property to the reporting manager (self-referencing)
    /// </summary>
    public Employee? Manager { get; private set; }

    /// <summary>
    /// Collection of employees managed by this employee (subordinates)
    /// </summary>
    private readonly List<Employee> _subordinates = [];

    public IReadOnlyCollection<EmployeeRole> EmployeeRoles
        => _employeeRoles.AsReadOnly();

    public IReadOnlyCollection<EmployeeTimeline> Timelines
        => _timelines.AsReadOnly();

    public IReadOnlyCollection<EmployeeDocument> Documents
        => _documents.AsReadOnly();

    public IReadOnlyCollection<Employee> Subordinates
        => _subordinates.AsReadOnly();

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

    public void Update(
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        Guid departmentId)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        DepartmentId = departmentId;
    }

    public void AssignDepartment(
        Guid departmentId)
    {
        DepartmentId = departmentId;
    }

    public void UpdatePhoneNumber(
        string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    /// <summary>
    /// Assigns a manager to this employee (hierarchical reporting relationship)
    /// </summary>
    public void AssignManager(Guid? managerId)
    {
        if (managerId == Id)
            throw new InvalidOperationException("An employee cannot be their own manager.");

        ManagerId = managerId;
    }

    /// <summary>
    /// Removes the manager assignment
    /// </summary>
    public void RemoveManager()
    {
        ManagerId = null;
    }

    public void AssignRole(
        Guid roleId)
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

    /// <summary>
    /// Adds a timeline event for this employee
    /// </summary>
    public void AddTimelineEvent(EmployeeTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);

        if (timeline.EmployeeId != Id)
            throw new InvalidOperationException("Timeline event must belong to this employee.");

        _timelines.Add(timeline);
    }

    /// <summary>
    /// Adds a document for this employee
    /// </summary>
    public void AddDocument(EmployeeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.EmployeeId != Id)
            throw new InvalidOperationException("Document must belong to this employee.");

        _documents.Add(document);
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