using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Reports;
using EmployeeLifecyclePortal.Application.Interfaces;

namespace EmployeeLifecyclePortal.Application.Queries.Reports;

public sealed class GetLeaveReportQuery : IRequest<ReportDataDto>
{
    public int? Year { get; set; }
    public Guid? EmployeeId { get; set; }

    public GetLeaveReportQuery(int? year = null, Guid? employeeId = null)
    {
        Year = year;
        EmployeeId = employeeId;
    }
}

public sealed class GetLeaveReportQueryHandler : IRequestHandler<GetLeaveReportQuery, ReportDataDto>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportDataDto> Handle(GetLeaveReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests.AsQueryable();

        if (request.Year.HasValue)
        {
            query = query.Where(l => l.StartDateUtc.Year == request.Year.Value || l.CreatedAtUtc.Year == request.Year.Value);
        }

        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            query = query.Where(l => l.EmployeeId == request.EmployeeId.Value);
        }

        var requests = await query.ToListAsync(cancellationToken);

        var approved = requests.Count(r => r.Status == "Approved");
        var pending = requests.Count(r => r.Status == "Pending");
        var rejected = requests.Count(r => r.Status == "Rejected");

        var chartData = new List<ReportChartDataDto>
        {
            new ReportChartDataDto
            {
                Label = "Leave Request Status Breakdown",
                Type = "pie",
                Labels = new List<string> { "Approved", "Pending", "Rejected" },
                Values = new List<double> { approved, pending, rejected },
                Color = "#F59E0B"
            }
        };

        return new ReportDataDto
        {
            Data = new Dictionary<string, object?>
            {
                { "TotalLeaveRequests", requests.Count },
                { "Approved", approved },
                { "Pending", pending },
                { "Rejected", rejected }
            },
            ChartData = chartData,
            Summary = new ReportSummaryDto
            {
                TotalRecords = requests.Count,
                KeyMetric = $"Approved: {approved}, Pending: {pending}"
            }
        };
    }
}
