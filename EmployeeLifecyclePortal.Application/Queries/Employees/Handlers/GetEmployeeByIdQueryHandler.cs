using EmployeeLifecyclePortal.Application.DTOs.Employees;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Employees.Handlers;

public sealed class GetEmployeeByIdQueryHandler
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeByIdQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(
        GetEmployeeByIdQuery request,
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

        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == employee.DepartmentId, cancellationToken);

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
            ManagerName = employee.Manager?.FullName,
            TeamLeadId = employee.TeamLeadId,
            TeamLeadName = employee.TeamLead?.FullName,
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