using EmployeeLifecyclePortal.Application.DTOs.ReadModels;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Employees.Handlers;

public sealed class GetEmployeeRolesQueryHandler
    : IRequestHandler<GetEmployeeRolesQuery, EmployeeRolesDto>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeRolesQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeRolesDto> Handle(
        GetEmployeeRolesQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(x => x.EmployeeRoles)
            .FirstOrDefaultAsync(
                x => x.Id == request.EmployeeId,
                cancellationToken);

        if (employee is null)
        {
            throw new InvalidOperationException(
                "Employee not found.");
        }

        var roleIds = employee.EmployeeRoles
            .Select(x => x.RoleId)
            .ToList();

        var roles = await _context.Roles
            .Where(x => roleIds.Contains(x.Id))
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        return new EmployeeRolesDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            Roles = roles
        };
    }
}