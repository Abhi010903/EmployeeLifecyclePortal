namespace EmployeeLifecyclePortal.Application.DTOs.Dashboard;

/// <summary>
/// Overall dashboard metrics combining summary, charts, and activity data.
/// This is the main DTO returned by the dashboard endpoint.
/// </summary>
public sealed class DashboardMetricsDto
{
    /// <summary>
    /// Summary statistics for key metrics.
    /// </summary>
    public DashboardSummaryDto Summary { get; set; } = new();

    /// <summary>
    /// Employee growth and attendance trend data (monthly).
    /// </summary>
    public List<EmployeeGrowthChartDto> EmployeeGrowthTrend { get; set; } = [];

    /// <summary>
    /// Department headcount breakdown.
    /// </summary>
    public List<DepartmentHeadcountDto> DepartmentHeadcount { get; set; } = [];

    /// <summary>
    /// Recent activities in the system.
    /// </summary>
    public List<RecentActivityDto> RecentActivities { get; set; } = [];
}
