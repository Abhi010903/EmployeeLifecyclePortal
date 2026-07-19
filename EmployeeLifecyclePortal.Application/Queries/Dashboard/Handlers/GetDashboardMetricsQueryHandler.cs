using EmployeeLifecyclePortal.Application.DTOs.Dashboard;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Dashboard.Handlers;

/// <summary>
/// Handles the GetDashboardMetricsQuery by combining results from all other dashboard queries.
/// This composite handler returns comprehensive dashboard data in a single call.
/// </summary>
public sealed class GetDashboardMetricsQueryHandler
    : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly IMediator _mediator;

    public GetDashboardMetricsQueryHandler(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<DashboardMetricsDto> Handle(
        GetDashboardMetricsQuery request,
        CancellationToken cancellationToken)
    {
        // Execute all queries in parallel for performance
        var summaryTask = _mediator.Send(
            new GetDashboardSummaryQuery(),
            cancellationToken);

        var growthTrendTask = _mediator.Send(
            new GetEmployeeGrowthTrendQuery(),
            cancellationToken);

        var departmentHeadcountTask = _mediator.Send(
            new GetDepartmentHeadcountQuery(),
            cancellationToken);

        var recentActivityTask = _mediator.Send(
            new GetRecentActivityQuery(Limit: 10),
            cancellationToken);

        // Wait for all queries to complete
        await Task.WhenAll(
            summaryTask,
            growthTrendTask,
            departmentHeadcountTask,
            recentActivityTask);

        return new DashboardMetricsDto
        {
            Summary = await summaryTask,
            EmployeeGrowthTrend = await growthTrendTask,
            DepartmentHeadcount = await departmentHeadcountTask,
            RecentActivities = await recentActivityTask
        };
    }
}
