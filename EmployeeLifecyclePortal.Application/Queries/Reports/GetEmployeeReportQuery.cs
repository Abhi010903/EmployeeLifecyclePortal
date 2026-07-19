using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Reports;

namespace EmployeeLifecyclePortal.Application.Queries.Reports;

public sealed class GetEmployeeReportQuery : IRequest<ReportDataDto>
{
    public int? DepartmentIdFilter { get; set; }
    public string? StatusFilter { get; set; }

    public GetEmployeeReportQuery(int? departmentId = null, string? status = null)
    {
        DepartmentIdFilter = departmentId;
        StatusFilter = status;
    }
}
