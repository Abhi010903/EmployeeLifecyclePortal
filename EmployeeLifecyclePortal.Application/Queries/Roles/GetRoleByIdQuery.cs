using EmployeeLifecyclePortal.Application.DTOs.Roles;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Roles;

public sealed record GetRoleByIdQuery(
    Guid Id)
    : IRequest<RoleDto>;