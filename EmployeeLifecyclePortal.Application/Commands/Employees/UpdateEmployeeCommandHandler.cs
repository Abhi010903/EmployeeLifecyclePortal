using EmployeeLifecyclePortal.Application.DTOs.Employees;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed class UpdateEmployeeCommandHandler
    : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(e => e.Manager)
            .Include(e => e.TeamLead)
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (employee is null)
        {
            throw new InvalidOperationException(
                "Employee not found.");
        }

        employee.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DepartmentId);

        if (request.ManagerId.HasValue)
        {
            if (request.ManagerId.Value != Guid.Empty)
                employee.AssignManager(request.ManagerId.Value);
            else
                employee.RemoveManager();
        }

        if (request.TeamLeadId.HasValue)
        {
            if (request.TeamLeadId.Value != Guid.Empty)
                employee.AssignTeamLead(request.TeamLeadId.Value);
            else
                employee.RemoveTeamLead();
        }

        // Update Role if provided
        if (request.RoleId.HasValue)
        {
            var existingRoles = await _context.EmployeeRoles
                .Where(er => er.EmployeeId == employee.Id)
                .ToListAsync(cancellationToken);

            _context.EmployeeRoles.RemoveRange(existingRoles);

            if (request.RoleId.Value != Guid.Empty)
            {
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == request.RoleId.Value, cancellationToken);

                if (role != null)
                {
                    _context.EmployeeRoles.Add(new EmployeeRole(employee.Id, role.Id));
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == employee.DepartmentId, cancellationToken);

        var manager = employee.ManagerId.HasValue
            ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == employee.ManagerId.Value, cancellationToken)
            : null;

        var teamLead = employee.TeamLeadId.HasValue
            ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == employee.TeamLeadId.Value, cancellationToken)
            : null;

        var assignedRoles = await _context.EmployeeRoles
            .Where(er => er.EmployeeId == employee.Id)
            .Join(_context.Roles,
                er => er.RoleId,
                r => r.Id,
                (er, r) => new { r.Id, r.Name })
            .ToListAsync(cancellationToken);

        var primaryRole = assignedRoles.FirstOrDefault();

        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Status = employee.Status.ToString(),
            DepartmentId = employee.DepartmentId,
            DepartmentName = department?.Name,
            ManagerId = employee.ManagerId,
            ManagerName = manager?.FullName,
            TeamLeadId = employee.TeamLeadId,
            TeamLeadName = teamLead?.FullName,
            RoleId = primaryRole?.Id,
            RoleName = primaryRole?.Name,
            Roles = assignedRoles.Select(r => r.Name).ToList(),
            CreatedAtUtc = employee.CreatedAtUtc,
            CreatedBy = employee.CreatedBy,
            LastModifiedAtUtc = employee.LastModifiedAtUtc,
            LastModifiedBy = employee.LastModifiedBy
        };
    }
}