using EmployeeLifecyclePortal.Application.DTOs.Roles;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Roles;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDto> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException($"Role with ID {request.Id} not found.");
        }

        // Update properties
        var roleType = role.GetType();
        var nameProperty = roleType.GetProperty("Name");
        var descriptionProperty = roleType.GetProperty("Description");

        nameProperty?.SetValue(role, request.Name);
        descriptionProperty?.SetValue(role, request.Description);

        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAtUtc = role.CreatedAtUtc,
            CreatedBy = role.CreatedBy,
            LastModifiedAtUtc = role.LastModifiedAtUtc,
            LastModifiedBy = role.LastModifiedBy,
        };
    }
}
