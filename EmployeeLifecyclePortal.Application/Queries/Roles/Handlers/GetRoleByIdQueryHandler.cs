using EmployeeLifecyclePortal.Application.DTOs.Roles;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Roles.Handlers;

public sealed class GetRoleByIdQueryHandler
    : IRequestHandler<GetRoleByIdQuery, RoleDto>
{
    private readonly IApplicationDbContext _context;

    public GetRoleByIdQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDto> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException(
                "Role not found.");
        }

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAtUtc = role.CreatedAtUtc,
            CreatedBy = role.CreatedBy,
            LastModifiedAtUtc = role.LastModifiedAtUtc,
            LastModifiedBy = role.LastModifiedBy
        };
    }
}