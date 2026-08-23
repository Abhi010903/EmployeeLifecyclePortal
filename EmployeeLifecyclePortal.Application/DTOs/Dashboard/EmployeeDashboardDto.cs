using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.DTOs.Tasks;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.DTOs.Dashboard;

public sealed class EmployeeDashboardDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string RoleName { get; set; } = "Employee";
    public string? ManagerName { get; set; }
    
    // Today Attendance
    public AttendanceDto? TodayAttendance { get; set; }
    public bool IsCheckedIn { get; set; }
    public decimal HoursWorkedToday { get; set; }

    // Work Tasks
    public int PendingTasksCount { get; set; }
    public int CompletedTasksCount { get; set; }
    public List<WorkTaskDto> AssignedTasks { get; set; } = new();

    // Leave Balances
    public int TotalLeaveDays { get; set; }
    public int UsedLeaveDays { get; set; }
    public int RemainingLeaveDays { get; set; }
    public List<LeaveBalanceDto> LeaveBalances { get; set; } = new();

    // Latest Payslip / Payroll
    public PayslipDto? LatestPayslip { get; set; }

    // Reimbursements
    public decimal PendingReimbursementAmount { get; set; }
    public List<ReimbursementDto> RecentReimbursements { get; set; } = new();

    // Assets
    public int AssignedAssetsCount { get; set; }
    public List<AssetAssignmentDto> AssignedAssets { get; set; } = new();

    // Upcoming Holidays
    public List<HolidaySummaryDto> UpcomingHolidays { get; set; } = new();
}

public sealed class HolidaySummaryDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
}
