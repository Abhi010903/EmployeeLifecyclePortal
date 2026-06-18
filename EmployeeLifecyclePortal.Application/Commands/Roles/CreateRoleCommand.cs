using EmployeeLifecyclePortal.Application.DTOs.Roles;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Roles;

public sealed record CreateRoleCommand(
    string Name,
    string Description)
    : IRequest<RoleDto>;