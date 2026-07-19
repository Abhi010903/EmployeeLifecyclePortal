using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard;

/// <summary>
/// Query to retrieve recent activities/audit log events.
/// Returns the latest HR actions performed in the system.
/// Used to populate the recent activity timeline on the dashboard.
/// </summary>
public sealed record GetRecentActivityQuery(
    /// <summary>
    /// Maximum number of activities to return. Default is 10.
    /// </summary>
    int Limit = 10
) : IRequest<List<RecentActivityDto>>;
