using EmployeeLifecyclePortal.Application.DTOs.Roles;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Roles;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    string Description)
    : IRequest<RoleDto>;
