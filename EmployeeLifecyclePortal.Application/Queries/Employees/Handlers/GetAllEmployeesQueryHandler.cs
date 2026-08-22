using EmployeeLifecyclePortal.Application.DTOs.Employees;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Employees.Handlers;

public sealed class GetAllEmployeesQueryHandler
    : IRequestHandler<GetAllEmployeesQuery, List<EmployeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEmployeesQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeDto>> Handle(
        GetAllEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await _context.Employees
            .Include(e => e.Manager)
            .Include(e => e.TeamLead)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);

        var departments = await _context.Departments
            .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);

        var employeeRoles = await _context.EmployeeRoles
            .Join(_context.Roles,
                er => er.RoleId,
                r => r.Id,
                (er, r) => new { er.EmployeeId, RoleId = r.Id, RoleName = r.Name })
            .ToListAsync(cancellationToken);

        var rolesByEmployee = employeeRoles
            .GroupBy(er => er.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return employees.Select(employee =>
        {
            rolesByEmployee.TryGetValue(employee.Id, out var rolesList);
            var primaryRole = rolesList?.FirstOrDefault();

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
                DepartmentName = departments.TryGetValue(employee.DepartmentId, out var deptName) ? deptName : "Unknown",
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager?.FullName,
                TeamLeadId = employee.TeamLeadId,
                TeamLeadName = employee.TeamLead?.FullName,
                RoleId = primaryRole?.RoleId,
                RoleName = primaryRole?.RoleName,
                Roles = rolesList?.Select(r => r.RoleName).ToList() ?? [],
                CreatedAtUtc = employee.CreatedAtUtc,
                CreatedBy = employee.CreatedBy,
                LastModifiedAtUtc = employee.LastModifiedAtUtc,
                LastModifiedBy = employee.LastModifiedBy
            };
        }).ToList();
    }
}