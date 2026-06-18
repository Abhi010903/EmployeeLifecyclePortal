using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Roles;

public sealed class DeleteRoleCommandHandler
    : IRequestHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        DeleteRoleCommand request,
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

        bool roleAssigned =
            await _context.EmployeeRoles.AnyAsync(
                x => x.RoleId == request.Id,
                cancellationToken);

        if (roleAssigned)
        {
            throw new InvalidOperationException(
                "Cannot delete role assigned to employees.");
        }

        _context.Roles.Remove(role);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}