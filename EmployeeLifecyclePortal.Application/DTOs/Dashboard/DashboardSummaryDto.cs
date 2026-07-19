namespace EmployeeLifecyclePortal.Application.DTOs.Dashboard;

/// <summary>
/// Summary statistics for the enterprise dashboard.
/// Contains key metrics about employees, attendance, and leave.
/// </summary>
public sealed class DashboardSummaryDto
{
    /// <summary>
    /// Total number of employees in the organization.
    /// </summary>
    public int TotalEmployees { get; set; }

    /// <summary>
    /// Number of actively employed employees.
    /// </summary>
    public int ActiveEmployees { get; set; }

    /// <summary>
    /// Number of inactive employees.
    /// </summary>
    public int InactiveEmployees { get; set; }

    /// <summary>
    /// Total number of departments.
    /// </summary>
    public int TotalDepartments { get; set; }

    /// <summary>
    /// Total number of roles in the system.
    /// </summary>
    public int TotalRoles { get; set; }

    /// <summary>
    /// Number of employees currently on leave.
    /// </summary>
    public int EmployeesOnLeave { get; set; }

    /// <summary>
    /// Number of pending leave requests awaiting approval.
    /// </summary>
    public int PendingLeaveRequests { get; set; }

    /// <summary>
    /// Number of employees present today.
    /// </summary>
    public int TodayAttendance { get; set; }

    /// <summary>
    /// Total payroll amount due (in currency units).
    /// </summary>
    public decimal TotalPayrollDue { get; set; }

    /// <summary>
    /// Percentage change in total employees compared to previous period.
    /// </summary>
    public decimal EmployeeTrend { get; set; }

    /// <summary>
    /// Percentage change in today's attendance compared to average.
    /// </summary>
    public decimal AttendanceTrend { get; set; }

    /// <summary>
    /// Percentage change in pending leave requests compared to previous period.
    /// </summary>
    public decimal LeaveTrend { get; set; }

    /// <summary>
    /// Percentage change in payroll due compared to previous period.
    /// </summary>
    public decimal PayrollTrend { get; set; }
}
