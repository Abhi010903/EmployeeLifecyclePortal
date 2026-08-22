namespace EmployeeLifecyclePortal.Application.DTOs.Dashboard;

/// <summary>
/// Summary statistics for the enterprise dashboard.
/// Contains key metrics about employees, attendance, leave, tasks, and staffing.
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
    /// Number of distinct employees present today.
    /// </summary>
    public int TodayAttendance { get; set; }

    /// <summary>
    /// Number of active work sessions currently open (checked in without checkout).
    /// </summary>
    public int ActiveWorkSessions { get; set; }

    /// <summary>
    /// Number of pending staffing requests from managers.
    /// </summary>
    public int PendingStaffingRequests { get; set; }

    /// <summary>
    /// Tasks breakdown
    /// </summary>
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CompletedTasks { get; set; }

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
