using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using EmployeeLifecyclePortal.Application.DTOs.Tasks;
using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Dashboard;
using EmployeeLifecyclePortal.Application.Queries.Attendance;
using EmployeeLifecyclePortal.Application.Queries.Payroll;
using EmployeeLifecyclePortal.Application.Queries.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Api.Controllers;

/// <summary>
/// Dashboard controller that provides enterprise analytics and metrics endpoints.
/// Returns real data from the database for dashboard visualization.
/// Requires Employee authorization (authenticated users).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public DashboardController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _context = context;
    }

    /// <summary>
    /// Gets personal dashboard metrics for the currently authenticated employee.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyDashboard(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == empId, cancellationToken);

        var department = employee != null
            ? await _context.Departments.FirstOrDefaultAsync(d => d.Id == employee.DepartmentId, cancellationToken)
            : null;

        var manager = employee?.ManagerId.HasValue == true
            ? await _context.Employees.FirstOrDefaultAsync(e => e.Id == employee.ManagerId.Value, cancellationToken)
            : null;

        var todayUtc = DateTime.UtcNow.Date;
        var todayAttendanceList = await _mediator.Send(new GetAttendanceByEmployeeQuery(empId), cancellationToken);
        var todayRecord = todayAttendanceList.FirstOrDefault(a => a.CheckInTimeUtc.Date == todayUtc);

        var tasks = await _mediator.Send(new GetWorkTasksQuery(EmployeeId: empId), cancellationToken);
        var balances = await _mediator.Send(new GetLeaveBalanceQuery(empId), cancellationToken);
        var payslips = await _mediator.Send(new GetPayslipsQuery(empId), cancellationToken);
        var reimbursements = await _mediator.Send(new GetReimbursementsQuery(empId), cancellationToken);

        var currentYear = DateTime.UtcNow.Year;
        var holidays = await _context.HolidayCalendars
            .Where(h => h.Year == currentYear && h.HolidayDate >= DateTime.UtcNow.Date)
            .OrderBy(h => h.HolidayDate)
            .Take(5)
            .Select(h => new HolidaySummaryDto
            {
                Name = h.HolidayName,
                Date = h.HolidayDate,
                Description = h.Description ?? ""
            })
            .ToListAsync(cancellationToken);

        var assetAssignments = await _context.AssetAssignments
            .Include(aa => aa.Asset)
            .Where(aa => aa.EmployeeId == empId && aa.ReturnedDateUtc == null)
            .Select(aa => new AssetAssignmentDto
            {
                Id = aa.Id,
                EmployeeId = aa.EmployeeId,
                EmployeeName = employee != null ? employee.FullName : "Employee",
                AssetId = aa.AssetId,
                AssetName = aa.Asset != null ? aa.Asset.AssetName : "Asset",
                AssetType = aa.Asset != null ? aa.Asset.AssetType : "Hardware",
                AssignedDateUtc = aa.AssignedDateUtc,
                Status = aa.Status,
                Notes = aa.Notes,
                CreatedAtUtc = aa.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var dto = new EmployeeDashboardDto
        {
            EmployeeId = empId,
            EmployeeName = employee?.FullName ?? "Employee",
            EmployeeCode = employee?.EmployeeCode ?? "",
            DepartmentName = department?.Name ?? "General",
            RoleName = _currentUserService.Role,
            ManagerName = manager?.FullName,
            TodayAttendance = todayRecord,
            IsCheckedIn = todayRecord != null && todayRecord.CheckOutTimeUtc == null,
            HoursWorkedToday = todayRecord?.HoursWorked ?? 0m,
            PendingTasksCount = tasks.Count(t => t.Status != "Completed"),
            CompletedTasksCount = tasks.Count(t => t.Status == "Completed"),
            AssignedTasks = tasks.Take(10).ToList(),
            TotalLeaveDays = balances.Sum(b => b.TotalDays),
            UsedLeaveDays = balances.Sum(b => b.UsedDays),
            RemainingLeaveDays = balances.Sum(b => b.RemainingDays),
            LeaveBalances = balances,
            LatestPayslip = payslips.FirstOrDefault(),
            PendingReimbursementAmount = reimbursements.Where(r => r.Status == "Pending").Sum(r => r.Amount),
            RecentReimbursements = reimbursements.Take(5).ToList(),
            AssignedAssetsCount = assetAssignments.Count,
            AssignedAssets = assetAssignments,
            UpcomingHolidays = holidays
        };

        return Ok(dto);
    }

    /// <summary>
    /// Gets comprehensive dashboard metrics including summary, charts, and recent activity.
    /// Endpoint: GET /api/dashboard/metrics
    /// </summary>
    [HttpGet("metrics")]
    [Authorize(Policy = Permissions.Manager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardMetrics(
        CancellationToken cancellationToken)
    {
        var metrics = await _mediator.Send(
            new GetDashboardMetricsQuery(),
            cancellationToken);

        return Ok(metrics);
    }

    /// <summary>
    /// Gets dashboard summary statistics only.
    /// Endpoint: GET /api/dashboard/summary
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Policy = Permissions.Manager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardSummary(
        CancellationToken cancellationToken)
    {
        var summary = await _mediator.Send(
            new GetDashboardSummaryQuery(),
            cancellationToken);

        return Ok(summary);
    }

    /// <summary>
    /// Gets employee growth trend data for the last 6 months.
    /// Endpoint: GET /api/dashboard/growth-trend
    /// Used to populate the employee growth and attendance trend line chart.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>
    /// 200 OK: List of EmployeeGrowthChartDto with monthly data (last 6 months)
    /// 401 Unauthorized: User not authenticated
    /// 500 Internal Server Error: Database or processing error
    /// </returns>
    [HttpGet("growth-trend")]
    [HttpGet("employee-growth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEmployeeGrowthTrend(
        CancellationToken cancellationToken)
    {
        var trend = await _mediator.Send(
            new GetEmployeeGrowthTrendQuery(),
            cancellationToken);

        return Ok(trend);
    }

    /// <summary>
    /// Gets department headcount breakdown.
    /// Endpoint: GET /api/dashboard/department-headcount
    /// Shows number of active employees per department.
    /// Used to populate the department headcount bar chart.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>
    /// 200 OK: List of DepartmentHeadcountDto sorted by employee count descending
    /// 401 Unauthorized: User not authenticated
    /// 500 Internal Server Error: Database or processing error
    /// </returns>
    [HttpGet("department-headcount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDepartmentHeadcount(
        CancellationToken cancellationToken)
    {
        var headcount = await _mediator.Send(
            new GetDepartmentHeadcountQuery(),
            cancellationToken);

        return Ok(headcount);
    }

    /// <summary>
    /// Gets recent activities from the audit log.
    /// Endpoint: GET /api/dashboard/recent-activity
    /// Returns the latest HR actions performed in the system.
    /// Query parameters:
    ///   - limit: Maximum number of activities to return (default: 10, max: 50)
    /// Used to populate the recent activity timeline on the dashboard.
    /// </summary>
    /// <param name="limit">Maximum number of recent activities to retrieve (default: 10, max: 50).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>
    /// 200 OK: List of RecentActivityDto sorted by date descending
    /// 401 Unauthorized: User not authenticated
    /// 500 Internal Server Error: Database or processing error
    /// </returns>
    [HttpGet("recent-activity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentActivity(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // Clamp limit between 1 and 50 for performance
        var clampedLimit = Math.Max(1, Math.Min(50, limit));

        var activity = await _mediator.Send(
            new GetRecentActivityQuery(clampedLimit),
            cancellationToken);

        return Ok(activity);
    }
}
