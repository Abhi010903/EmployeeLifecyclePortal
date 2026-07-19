using EmployeeLifecyclePortal.Application.DTOs.Departments;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string Description)
    : IRequest<DepartmentDto>;
