using EmployeeLifecyclePortal.Application.DTOs.ReadModels;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Departments.Handlers;

public sealed class GetDepartmentEmployeesQueryHandler
    : IRequestHandler<GetDepartmentEmployeesQuery, DepartmentEmployeesDto>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentEmployeesQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentEmployeesDto> Handle(
        GetDepartmentEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(
                x => x.Id == request.DepartmentId,
                cancellationToken);

        if (department is null)
        {
            throw new InvalidOperationException(
                "Department not found.");
        }

        var employees = await _context.Employees
            .Where(x => x.DepartmentId == request.DepartmentId)
            .Select(x => x.FullName)
            .ToListAsync(cancellationToken);

        return new DepartmentEmployeesDto
        {
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            Employees = employees
        };
    }
}