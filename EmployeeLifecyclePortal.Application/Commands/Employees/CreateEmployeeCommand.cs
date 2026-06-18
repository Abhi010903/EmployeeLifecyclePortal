using EmployeeLifecyclePortal.Application.DTOs.Employees;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed record CreateEmployeeCommand(
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Guid DepartmentId)
    : IRequest<EmployeeDto>;