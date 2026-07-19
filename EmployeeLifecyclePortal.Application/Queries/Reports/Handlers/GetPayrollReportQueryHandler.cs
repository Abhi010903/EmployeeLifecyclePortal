using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Reports;
using EmployeeLifecyclePortal.Application.Interfaces;

namespace EmployeeLifecyclePortal.Application.Queries.Reports.Handlers;

public sealed class GetPayrollReportQueryHandler : IRequestHandler<GetPayrollReportQuery, ReportDataDto>
{
    private readonly IApplicationDbContext _context;

    public GetPayrollReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportDataDto> Handle(
        GetPayrollReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Payslips
            .Where(p => p.Month == request.Month && p.Year == request.Year)
            .AsQueryable();

        if (request.EmployeeIdFilter.HasValue)
            query = query.Where(p => p.EmployeeId == request.EmployeeIdFilter);

        var payslips = await query
            .Include(p => p.Employee)
            .ToListAsync(cancellationToken);

        var totalGross = payslips.Sum(p => p.GrossSalary);
        var totalDeductions = payslips.Sum(p => p.Deductions);
        var totalNet = payslips.Sum(p => p.NetSalary);
        var avgGross = payslips.Count > 0 ? totalGross / payslips.Count : 0;

        var costBreakdown = new ReportChartDataDto
        {
            Label = "Cost Breakdown",
            Type = "bar",
            Labels = new List<string> { "Gross", "Deductions", "Net" },
            Values = new List<double> { (double)totalGross, (double)totalDeductions, (double)totalNet },
            Color = "#06B6D4"
        };

        return new ReportDataDto
        {
            Data = new Dictionary<string, object?>
            {
                { "Month", request.Month },
                { "Year", request.Year },
                { "TotalEmployees", payslips.Count },
                { "TotalGrossSalary", totalGross },
                { "TotalDeductions", totalDeductions },
                { "TotalNetSalary", totalNet },
                { "AverageGrossSalary", avgGross }
            },
            ChartData = new List<ReportChartDataDto> { costBreakdown },
            Summary = new ReportSummaryDto
            {
                TotalRecords = payslips.Count,
                TotalAmount = totalNet,
                AverageAmount = avgGross,
                KeyMetric = $"Total Payroll: ₹{totalNet:N0}"
            }
        };
    }
}
