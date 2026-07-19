using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard.Handlers;

/// <summary>
/// Handles the GetDepartmentHeadcountQuery to retrieve active employee count per department.
/// Used for the department headcount bar chart.
/// </summary>
public sealed class GetDepartmentHeadcountQueryHandler
    : IRequestHandler<GetDepartmentHeadcountQuery, List<DepartmentHeadcountDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentHeadcountQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentHeadcountDto>> Handle(
        GetDepartmentHeadcountQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _context.Departments
            .Select(d => new DepartmentHeadcountDto
            {
                DepartmentName = d.Name,
                EmployeeCount = d.Employees
                    .Count(e => e.Status == EmploymentStatus.Active)
            })
            .OrderByDescending(x => x.EmployeeCount)
            .ToListAsync(cancellationToken);

        return result;
    }
}
