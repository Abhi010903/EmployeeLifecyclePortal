using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard.Handlers;

/// <summary>
/// Handles the GetDashboardSummaryQuery to retrieve key metrics from the database.
/// Calculates counts for employees, departments, leave, attendance, tasks, staffing, and payroll.
/// </summary>
public sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        // Total employees and active/inactive breakdown
        var totalEmployees = await _context.Employees.CountAsync(cancellationToken);
        var activeEmployees = await _context.Employees
            .CountAsync(e => e.Status == EmploymentStatus.Active, cancellationToken);
        var inactiveEmployees = totalEmployees - activeEmployees;

        // Departments and roles
        var totalDepartments = await _context.Departments.CountAsync(cancellationToken);
        var totalRoles = await _context.Roles.CountAsync(cancellationToken);

        // Leave metrics
        int employeesOnLeave = 0;
        int pendingLeaveRequests = 0;
        try
        {
            var nowUtc = DateTime.UtcNow;
            employeesOnLeave = await _context.LeaveRequests
                .Where(lr => (lr.Status == "Approved" || lr.Status == "ManagerApproved") &&
                             lr.StartDateUtc <= nowUtc &&
                             lr.EndDateUtc >= nowUtc)
                .Select(lr => lr.EmployeeId)
                .Distinct()
                .CountAsync(cancellationToken);

            pendingLeaveRequests = await _context.LeaveRequests
                .CountAsync(lr => lr.Status == "Pending" || lr.Status == "ManagerApproved", cancellationToken);
        }
        catch
        {
            // LeaveRequests table fallback
        }

        // Today's attendance (distinct check-ins for today)
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        int todayAttendance = 0;
        int activeWorkSessions = 0;
        try
        {
            todayAttendance = await _context.Attendances
                .Where(a => a.CheckInTimeUtc >= todayStart && a.CheckInTimeUtc < todayEnd)
                .Select(a => a.EmployeeId)
                .Distinct()
                .CountAsync(cancellationToken);

            activeWorkSessions = await _context.Attendances
                .CountAsync(a => a.CheckOutTimeUtc == null, cancellationToken);
        }
        catch
        {
            // Attendances table fallback
        }

        // Tasks breakdown
        int pendingTasks = 0;
        int inProgressTasks = 0;
        int overdueTasks = 0;
        int completedTasks = 0;
        try
        {
            var allTasks = await _context.WorkTasks.AsNoTracking().ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;
            pendingTasks = allTasks.Count(t => t.Status == "Pending");
            inProgressTasks = allTasks.Count(t => t.Status == "InProgress");
            completedTasks = allTasks.Count(t => t.Status == "Completed");
            overdueTasks = allTasks.Count(t => t.DeadlineUtc < now && t.Status != "Completed");
        }
        catch
        {
            // WorkTasks table fallback
        }

        // Staffing requests count
        int pendingStaffingRequests = 0;
        try
        {
            pendingStaffingRequests = await _context.StaffingRequests
                .CountAsync(sr => sr.Status == "Pending", cancellationToken);
        }
        catch
        {
            // StaffingRequests table fallback
        }

        // Payroll due
        decimal totalPayrollDue = 0;
        try
        {
            totalPayrollDue = await _context.Payslips
                .Where(p => p.Status == "Generated" || p.Status == "Pending")
                .SumAsync(p => p.NetSalary, cancellationToken);
        }
        catch
        {
            // Payslips table fallback
        }

        // Calculate trends
        var previousMonthStart = DateTime.UtcNow.AddMonths(-1).Date;
        var currentMonthStart = DateTime.UtcNow.Date;

        var previousMonthEmployeesCount = await _context.Employees
            .Where(e => e.CreatedAtUtc < currentMonthStart && e.Status == EmploymentStatus.Active)
            .CountAsync(cancellationToken);

        var employeeTrend = previousMonthEmployeesCount > 0
            ? ((decimal)(activeEmployees - previousMonthEmployeesCount) / previousMonthEmployeesCount) * 100
            : 0;

        decimal attendanceTrend = 0;
        try
        {
            var averageDailyAttendance = await _context.Attendances
                .Where(a => a.CheckInTimeUtc >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(a => a.CheckInTimeUtc.Date)
                .Select(g => g.Select(a => a.EmployeeId).Distinct().Count())
                .AverageAsync(cancellationToken);

            attendanceTrend = averageDailyAttendance > 0
                ? ((decimal)todayAttendance - (decimal)averageDailyAttendance) / (decimal)averageDailyAttendance * 100
                : 0;
        }
        catch
        {
        }

        decimal leaveTrend = 0;
        try
        {
            var previousPeriodPendingLeave = await _context.LeaveRequests
                .Where(lr => (lr.Status == "Pending" || lr.Status == "ManagerApproved") &&
                             lr.CreatedAtUtc < currentMonthStart)
                .CountAsync(cancellationToken);

            leaveTrend = previousPeriodPendingLeave > 0
                ? ((decimal)(pendingLeaveRequests - previousPeriodPendingLeave) / previousPeriodPendingLeave) * 100
                : 0;
        }
        catch
        {
        }

        decimal payrollTrend = 0;
        try
        {
            var previousMonthPayrollDue = await _context.Payslips
                .Where(p => (p.Status == "Generated" || p.Status == "Pending") &&
                            p.CreatedAtUtc >= previousMonthStart &&
                            p.CreatedAtUtc < currentMonthStart)
                .SumAsync(p => p.NetSalary, cancellationToken);

            payrollTrend = previousMonthPayrollDue > 0
                ? ((decimal)(totalPayrollDue - previousMonthPayrollDue) / previousMonthPayrollDue) * 100
                : 0;
        }
        catch
        {
        }

        return new DashboardSummaryDto
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            InactiveEmployees = inactiveEmployees,
            TotalDepartments = totalDepartments,
            TotalRoles = totalRoles,
            EmployeesOnLeave = employeesOnLeave,
            PendingLeaveRequests = pendingLeaveRequests,
            TodayAttendance = todayAttendance,
            ActiveWorkSessions = activeWorkSessions,
            PendingStaffingRequests = pendingStaffingRequests,
            PendingTasks = pendingTasks,
            InProgressTasks = inProgressTasks,
            OverdueTasks = overdueTasks,
            CompletedTasks = completedTasks,
            TotalPayrollDue = totalPayrollDue,
            EmployeeTrend = Math.Round(employeeTrend, 2),
            AttendanceTrend = Math.Round(attendanceTrend, 2),
            LeaveTrend = Math.Round(leaveTrend, 2),
            PayrollTrend = Math.Round(payrollTrend, 2)
        };
    }
}
