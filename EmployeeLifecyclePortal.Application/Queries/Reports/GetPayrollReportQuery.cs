using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Reports;

namespace EmployeeLifecyclePortal.Application.Queries.Reports;

public sealed class GetPayrollReportQuery : IRequest<ReportDataDto>
{
    public int Month { get; set; }
    public int Year { get; set; }
    public Guid? EmployeeIdFilter { get; set; }

    public GetPayrollReportQuery(int month, int year, Guid? employeeId = null)
    {
        Month = month;
        Year = year;
        EmployeeIdFilter = employeeId;
    }
}
