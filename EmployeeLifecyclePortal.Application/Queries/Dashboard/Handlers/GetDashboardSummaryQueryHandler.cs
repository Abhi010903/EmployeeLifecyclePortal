using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard.Handlers;

/// <summary>
/// Handles the GetDashboardSummaryQuery to retrieve key metrics from the database.
/// Calculates counts for employees, departments, leave, attendance, and payroll.
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

        // Leave metrics (use 0 if LeaveRequests table doesn't exist)
        int employeesOnLeave = 0;
        int pendingLeaveRequests = 0;
        try
        {
            employeesOnLeave = await _context.LeaveRequests
                .Where(lr => lr.Status == "Approved" &&
                             lr.StartDateUtc <= DateTime.UtcNow &&
                             lr.EndDateUtc >= DateTime.UtcNow)
                .Select(lr => lr.EmployeeId)
                .Distinct()
                .CountAsync(cancellationToken);

            pendingLeaveRequests = await _context.LeaveRequests
                .CountAsync(lr => lr.Status == "Pending", cancellationToken);
        }
        catch
        {
            // LeaveRequests table may not exist in this database schema
        }

        // Today's attendance (check-ins for today) (use 0 if Attendances table doesn't exist)
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        int todayAttendance = 0;
        try
        {
            todayAttendance = await _context.Attendances
                .Where(a => a.CheckInTimeUtc >= todayStart && a.CheckInTimeUtc < todayEnd)
                .Select(a => a.EmployeeId)
                .Distinct()
                .CountAsync(cancellationToken);
        }
        catch
        {
            // Attendances table may not exist in this database schema
        }

        // Payroll due (sum of all pending/unpaid salaries from Payslips) (use 0 if Payslips table doesn't exist)
        decimal totalPayrollDue = 0;
        try
        {
            totalPayrollDue = await _context.Payslips
                .Where(p => p.Status == "Generated" || p.Status == "Pending")
                .SumAsync(p => p.NetSalary, cancellationToken);
        }
        catch
        {
            // Payslips table may not exist in this database schema
        }

        // Calculate trends (comparing with previous month/period)
        var previousMonthStart = DateTime.UtcNow.AddMonths(-1).Date;
        var currentMonthStart = DateTime.UtcNow.Date;

        var previousMonthEmployeesCount = await _context.Employees
            .Where(e => e.CreatedAtUtc < currentMonthStart && e.Status == EmploymentStatus.Active)
            .CountAsync(cancellationToken);

        var employeeTrend = previousMonthEmployeesCount > 0
            ? ((decimal)(activeEmployees - previousMonthEmployeesCount) / previousMonthEmployeesCount) * 100
            : 0;

        // Attendance trend (compared to average)
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
            // Attendances table may not exist
        }

        // Leave trend (pending vs previous period)
        decimal leaveTrend = 0;
        try
        {
            var previousPeriodPendingLeave = await _context.LeaveRequests
                .Where(lr => lr.Status == "Pending" &&
                             lr.CreatedAtUtc < currentMonthStart)
                .CountAsync(cancellationToken);

            leaveTrend = previousPeriodPendingLeave > 0
                ? ((decimal)(pendingLeaveRequests - previousPeriodPendingLeave) / previousPeriodPendingLeave) * 100
                : 0;
        }
        catch
        {
            // LeaveRequests table may not exist
        }

        // Payroll trend (compared to previous month)
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
            // Payslips table may not exist
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
            TotalPayrollDue = totalPayrollDue,
            EmployeeTrend = Math.Round(employeeTrend, 2),
            AttendanceTrend = Math.Round(attendanceTrend, 2),
            LeaveTrend = Math.Round(leaveTrend, 2),
            PayrollTrend = Math.Round(payrollTrend, 2)
        };
    }
}
