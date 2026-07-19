namespace EmployeeLifecyclePortal.Application.DTOs.Dashboard;

/// <summary>
/// Department headcount data for bar chart visualization.
/// Shows number of employees per department.
/// </summary>
public sealed class DepartmentHeadcountDto
{
    /// <summary>
    /// Department name.
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// Number of active employees in this department.
    /// </summary>
    public int EmployeeCount { get; set; }
}
