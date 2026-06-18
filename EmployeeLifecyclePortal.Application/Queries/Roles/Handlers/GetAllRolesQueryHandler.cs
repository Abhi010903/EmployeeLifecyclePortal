using EmployeeLifecyclePortal.Application.DTOs.Roles;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Roles.Handlers;

public sealed class GetAllRolesQueryHandler
    : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllRolesQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Roles
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);
    }
}