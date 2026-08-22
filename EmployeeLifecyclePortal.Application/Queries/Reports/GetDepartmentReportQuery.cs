using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Reports;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Enums;

namespace EmployeeLifecyclePortal.Application.Queries.Reports;

public sealed class GetDepartmentReportQuery : IRequest<ReportDataDto>
{
    public int? Month { get; set; }
    public int? Year { get; set; }

    public GetDepartmentReportQuery(int? month = null, int? year = null)
    {
        Month = month;
        Year = year;
    }
}

public sealed class GetDepartmentReportQueryHandler : IRequestHandler<GetDepartmentReportQuery, ReportDataDto>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportDataDto> Handle(GetDepartmentReportQuery request, CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .Include(d => d.Employees)
            .ToListAsync(cancellationToken);

        var labels = new List<string>();
        var values = new List<double>();
        var totalHeadcount = 0;

        foreach (var dept in departments)
        {
            var count = dept.Employees.Count(e => e.Status == EmploymentStatus.Active);
            labels.Add(dept.Name);
            values.Add(count);
            totalHeadcount += count;
        }

        var chartData = new List<ReportChartDataDto>
        {
            new ReportChartDataDto
            {
                Label = "Department Headcount Distribution",
                Type = "bar",
                Labels = labels,
                Values = values,
                Color = "#10B981"
            }
        };

        var data = new Dictionary<string, object?>
        {
            { "TotalDepartments", departments.Count },
            { "TotalActiveEmployees", totalHeadcount }
        };

        foreach (var dept in departments)
        {
            data[dept.Name] = dept.Employees.Count(e => e.Status == EmploymentStatus.Active);
        }

        return new ReportDataDto
        {
            Data = data,
            ChartData = chartData,
            Summary = new ReportSummaryDto
            {
                TotalRecords = departments.Count,
                KeyMetric = $"Total Depts: {departments.Count}, Headcount: {totalHeadcount}"
            }
        };
    }
}
