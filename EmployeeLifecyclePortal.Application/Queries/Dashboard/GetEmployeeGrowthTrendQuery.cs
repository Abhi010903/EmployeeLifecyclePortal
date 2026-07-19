using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard;

/// <summary>
/// Query to retrieve employee growth trend data (monthly).
/// Returns historical data for the last 6 months showing employee count and attendance.
/// Used to populate the employee growth trend line chart.
/// </summary>
public sealed record GetEmployeeGrowthTrendQuery
    : IRequest<List<EmployeeGrowthChartDto>>;
