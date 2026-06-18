using EmployeeLifecyclePortal.Application.DTOs.EmployeeRoles;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.EmployeeRoles;

public sealed record AssignRoleToEmployeeCommand(
    Guid EmployeeId,
    Guid RoleId)
    : IRequest<AssignRoleDto>;