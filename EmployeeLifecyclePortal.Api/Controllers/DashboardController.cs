using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets comprehensive dashboard metrics including summary, charts, and recent activity.
    /// Endpoint: GET /api/dashboard/metrics
    /// Returns all data needed to render the enterprise dashboard.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>
    /// 200 OK: DashboardMetricsDto containing summary, growth trend, department headcount, and recent activities
    /// 401 Unauthorized: User not authenticated
    /// 500 Internal Server Error: Database or processing error
    /// </returns>
    [HttpGet("metrics")]
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
    /// Lightweight endpoint for individual card updates.
    /// Contains counts for employees, departments, leave, attendance, and payroll with trends.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>
    /// 200 OK: DashboardSummaryDto with key metrics and percentage trends
    /// 401 Unauthorized: User not authenticated
    /// 500 Internal Server Error: Database or processing error
    /// </returns>
    [HttpGet("summary")]
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
