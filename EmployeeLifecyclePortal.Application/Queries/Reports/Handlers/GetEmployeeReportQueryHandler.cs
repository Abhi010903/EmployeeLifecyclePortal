using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Reports;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Enums;

namespace EmployeeLifecyclePortal.Application.Queries.Reports.Handlers;

public sealed class GetEmployeeReportQueryHandler : IRequestHandler<GetEmployeeReportQuery, ReportDataDto>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportDataDto> Handle(
        GetEmployeeReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrEmpty(request.StatusFilter))
        {
            if (Enum.TryParse<EmploymentStatus>(request.StatusFilter, out var status))
                query = query.Where(e => e.Status == status);
        }

        var employees = await query.ToListAsync(cancellationToken);

        var activeCount = employees.Count(e => e.Status == EmploymentStatus.Active);
        var inactiveCount = employees.Count(e => e.Status == EmploymentStatus.Inactive);
        var terminatedCount = employees.Count(e => e.Status == EmploymentStatus.Terminated);

        var statusData = new List<ReportChartDataDto>
        {
            new ReportChartDataDto
            {
                Label = "Employee Status Distribution",
                Type = "pie",
                Labels = new List<string> { "Active", "Inactive", "Terminated" },
                Values = new List<double> { activeCount, inactiveCount, terminatedCount },
                Color = "#3B82F6"
            }
        };

        return new ReportDataDto
        {
            Data = new Dictionary<string, object?>
            {
                { "TotalEmployees", employees.Count },
                { "ActiveEmployees", activeCount },
                { "InactiveEmployees", inactiveCount },
                { "TerminatedEmployees", terminatedCount },
            },
            ChartData = statusData,
            Summary = new ReportSummaryDto
            {
                TotalRecords = employees.Count,
                KeyMetric = $"Active: {activeCount}"
            }
        };
    }
}
