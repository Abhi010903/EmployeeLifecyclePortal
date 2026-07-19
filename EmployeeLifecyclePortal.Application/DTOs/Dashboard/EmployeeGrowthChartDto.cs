namespace EmployeeLifecyclePortal.Application.DTOs.Dashboard;

/// <summary>
/// Chart data for employee growth trend (monthly).
/// Used for visualizing employee count and attendance trends over time.
/// </summary>
public sealed class EmployeeGrowthChartDto
{
    /// <summary>
    /// Month label (e.g., "Jan", "Feb", "Mar").
    /// </summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// Total employee count for this month.
    /// </summary>
    public int TotalEmployees { get; set; }

    /// <summary>
    /// Total attendance count for this month (average or actual).
    /// </summary>
    public int TotalAttendance { get; set; }
}
