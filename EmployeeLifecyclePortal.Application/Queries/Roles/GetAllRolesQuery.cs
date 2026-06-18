using EmployeeLifecyclePortal.Application.DTOs.Roles;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Roles;

public sealed record GetAllRolesQuery
    : IRequest<List<RoleDto>>;