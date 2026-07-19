using EmployeeLifecyclePortal.Application.DTOs.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Performance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Performance.Handlers;

public sealed class GetEmployeeKPIsQueryHandler
    : IRequestHandler<GetEmployeeKPIsQuery, List<KPIDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeKPIsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<KPIDto>> Handle(
        GetEmployeeKPIsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.KPIs
            .Include(k => k.Employee)
            .Where(k => k.EmployeeId == request.EmployeeId)
            .Select(kpi => new KPIDto
            {
                Id = kpi.Id,
                EmployeeId = kpi.EmployeeId,
                EmployeeName = kpi.Employee != null 
                    ? $"{kpi.Employee.FirstName} {kpi.Employee.LastName}"
                    : "Unknown",
                Name = kpi.Name,
                Target = kpi.Target,
                Achieved = kpi.Achieved,
                Year = kpi.Year,
                AchievementPercentage = (decimal)kpi.GetAchievementPercentage(),
                CreatedAtUtc = kpi.CreatedAtUtc,
                CreatedBy = kpi.CreatedBy,
                LastModifiedAtUtc = kpi.LastModifiedAtUtc,
                LastModifiedBy = kpi.LastModifiedBy
            })
            .OrderByDescending(k => k.Year)
            .ThenBy(k => k.Name)
            .ToListAsync(cancellationToken);
    }
}
