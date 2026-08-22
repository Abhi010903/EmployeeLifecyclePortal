using EmployeeLifecyclePortal.Application.DTOs.Employees;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed record UpdateEmployeeCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Guid DepartmentId,
    Guid? RoleId = null,
    Guid? ManagerId = null,
    Guid? TeamLeadId = null)
    : IRequest<EmployeeDto>;