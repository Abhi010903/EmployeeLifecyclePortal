using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard;

/// <summary>
/// Query to retrieve comprehensive dashboard metrics including summary stats, charts, and recent activity.
/// Returns all data needed to render the enterprise dashboard.
/// </summary>
public sealed record GetDashboardMetricsQuery
    : IRequest<DashboardMetricsDto>;
