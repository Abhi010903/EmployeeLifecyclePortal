using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Reports;
using EmployeeLifecyclePortal.Application.Interfaces;

namespace EmployeeLifecyclePortal.Application.Queries.Reports.Handlers;

public sealed class GetAttendanceReportQueryHandler : IRequestHandler<GetAttendanceReportQuery, ReportDataDto>
{
    private readonly IApplicationDbContext _context;

    public GetAttendanceReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportDataDto> Handle(
        GetAttendanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Attendances
            .Where(a => a.CheckInTimeUtc >= request.StartDateUtc && a.CheckInTimeUtc <= request.EndDateUtc)
            .AsQueryable();

        if (request.EmployeeIdFilter.HasValue)
            query = query.Where(a => a.EmployeeId == request.EmployeeIdFilter);

        var records = await query
            .Include(a => a.Employee)
            .ToListAsync(cancellationToken);

        var presentCount = records.Count(r => r.Status == "Present");
        var absentCount = records.Count(r => r.Status == "Absent");
        var leaveCount = records.Count(r => r.Status == "Leave");

        var dailyData = records
            .GroupBy(r => r.CheckInTimeUtc.Date)
            .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToList();

        var attendanceChart = new ReportChartDataDto
        {
            Label = "Attendance Status",
            Type = "doughnut",
            Labels = new List<string> { "Present", "Absent", "Leave" },
            Values = new List<double> { presentCount, absentCount, leaveCount },
            Color = "#F59E0B"
        };

        var dailyChart = new ReportChartDataDto
        {
            Label = "Daily Attendance Trend",
            Type = "line",
            Labels = dailyData.Select(d => d.Date).ToList(),
            Values = dailyData.Select(d => (double)d.Count).ToList(),
            Color = "#8B5CF6"
        };

        return new ReportDataDto
        {
            Data = new Dictionary<string, object?>
            {
                { "TotalRecords", records.Count },
                { "PresentCount", presentCount },
                { "AbsentCount", absentCount },
                { "LeaveCount", leaveCount },
                { "AttendancePercentage", records.Count > 0 ? (presentCount * 100.0 / records.Count) : 0 }
            },
            ChartData = new List<ReportChartDataDto> { attendanceChart, dailyChart },
            Summary = new ReportSummaryDto
            {
                TotalRecords = records.Count,
                KeyMetric = $"Present: {presentCount}/{records.Count}"
            }
        };
    }
}
