using EmployeeLifecyclePortal.Application.DTOs;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Application.Queries.Employees;

public sealed class GetEmployeeProfileQueryHandler
    : IRequestHandler<GetEmployeeProfileQuery, EmployeeProfileDto>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<GetEmployeeProfileQueryHandler> _logger;

    public GetEmployeeProfileQueryHandler(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        ILogger<GetEmployeeProfileQueryHandler> logger)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<EmployeeProfileDto> Handle(
        GetEmployeeProfileQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(
            request.EmployeeId,
            cancellationToken);

        if (employee is null)
            throw new InvalidOperationException(
                $"Employee with ID {request.EmployeeId} not found.");

        var department = await _departmentRepository.GetByIdAsync(
            employee.DepartmentId,
            cancellationToken);

        string? managerName = null;
        if (employee.ManagerId.HasValue)
        {
            var manager = await _employeeRepository.GetByIdAsync(
                employee.ManagerId.Value,
                cancellationToken);
            managerName = manager?.FullName;
        }

        _logger.LogInformation(
            "Employee profile retrieved — Employee: {EmployeeId} | Name: {FullName}",
            request.EmployeeId,
            employee.FullName);

        return new EmployeeProfileDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Status = employee.Status.ToString(),
            DepartmentId = employee.DepartmentId,
            DepartmentName = department?.Name,
            ManagerId = employee.ManagerId,
            ManagerName = managerName,
            Roles = employee.EmployeeRoles.Select(x => new EmployeeRoleDto
            {
                RoleId = x.RoleId,
                RoleName = "Unknown",
                RoleDescription = string.Empty
            }).ToList(),
            CreatedAtUtc = employee.CreatedAtUtc,
            CreatedBy = employee.CreatedBy,
            LastModifiedAtUtc = employee.LastModifiedAtUtc,
            LastModifiedBy = employee.LastModifiedBy,
            TimelineEventsCount = employee.Timelines.Count,
            DocumentsCount = employee.Documents.Count,
            SubordinatesCount = employee.Subordinates.Count
        };
    }
}
