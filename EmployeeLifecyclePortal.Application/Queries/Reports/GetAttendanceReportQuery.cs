using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Reports;

namespace EmployeeLifecyclePortal.Application.Queries.Reports;

public sealed class GetAttendanceReportQuery : IRequest<ReportDataDto>
{
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public Guid? EmployeeIdFilter { get; set; }

    public GetAttendanceReportQuery(DateTime startDate, DateTime endDate, Guid? employeeId = null)
    {
        StartDateUtc = startDate;
        EndDateUtc = endDate;
        EmployeeIdFilter = employeeId;
    }
}
