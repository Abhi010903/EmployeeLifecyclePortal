using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard;

/// <summary>
/// Query to retrieve department headcount breakdown.
/// Shows number of active employees per department.
/// Used to populate the department headcount bar chart.
/// </summary>
public sealed record GetDepartmentHeadcountQuery
    : IRequest<List<DepartmentHeadcountDto>>;
