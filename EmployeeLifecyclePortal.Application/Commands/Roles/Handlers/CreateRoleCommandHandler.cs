using EmployeeLifecyclePortal.Application.DTOs.Roles;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Roles.Handlers;

public sealed class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDto> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = new Role(
            request.Name,
            request.Description);

        _context.Roles.Add(role);

        await _context.SaveChangesAsync(cancellationToken);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        };
    }
}