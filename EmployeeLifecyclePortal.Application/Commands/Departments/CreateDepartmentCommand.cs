using EmployeeLifecyclePortal.Application.DTOs.Departments;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed record CreateDepartmentCommand(
    string Name,
    string Description)
    : IRequest<DepartmentDto>;