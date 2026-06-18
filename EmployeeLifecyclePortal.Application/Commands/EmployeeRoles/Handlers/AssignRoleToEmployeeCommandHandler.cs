using EmployeeLifecyclePortal.Application.DTOs.EmployeeRoles;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.EmployeeRoles.Handlers;

public sealed class AssignRoleToEmployeeCommandHandler
    : IRequestHandler<AssignRoleToEmployeeCommand, AssignRoleDto>
{
    private readonly IApplicationDbContext _context;

    public AssignRoleToEmployeeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssignRoleDto> Handle(
        AssignRoleToEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employeeExists = await _context.Employees
            .AnyAsync(
                x => x.Id == request.EmployeeId,
                cancellationToken);

        if (!employeeExists)
        {
            throw new InvalidOperationException(
                "Employee not found.");
        }

        var roleExists = await _context.Roles
            .AnyAsync(
                x => x.Id == request.RoleId,
                cancellationToken);

        if (!roleExists)
        {
            throw new InvalidOperationException(
                "Role not found.");
        }

        var alreadyAssigned = await _context.EmployeeRoles
            .AnyAsync(
                x => x.EmployeeId == request.EmployeeId &&
                     x.RoleId == request.RoleId,
                cancellationToken);

        if (alreadyAssigned)
        {
            throw new InvalidOperationException(
                "Role is already assigned to employee.");
        }

        var employeeRole = new EmployeeRole(
            request.EmployeeId,
            request.RoleId);

        _context.EmployeeRoles.Add(employeeRole);

        await _context.SaveChangesAsync(cancellationToken);

        return new AssignRoleDto
        {
            EmployeeId = request.EmployeeId,
            RoleId = request.RoleId
        };
    }
}