using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Performance;
using EmployeeLifecyclePortal.Application.Queries.Performance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class PerformanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public PerformanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Goals endpoints
    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal(
        CreateGoalCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("goals")]
    public async Task<IActionResult> GetAllGoals(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAllGoalsQuery(),
            cancellationToken));
    }

    [HttpGet("goals/employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeGoals(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetEmployeeGoalsQuery(employeeId),
            cancellationToken));
    }

    [HttpPut("goals/{goalId:guid}/progress")]
    public async Task<IActionResult> UpdateGoalProgress(
        Guid goalId,
        [FromBody] UpdateGoalProgressRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new UpdateGoalProgressCommand(goalId, request.ProgressPercentage),
            cancellationToken));
    }

    [HttpPut("goals/{goalId:guid}/complete")]
    public async Task<IActionResult> CompleteGoal(
        Guid goalId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new CompleteGoalCommand(goalId),
            cancellationToken));
    }

    // Reviews endpoints
    [HttpPost("reviews")]
    public async Task<IActionResult> SubmitReview(
        SubmitReviewCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> GetAllReviews(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAllReviewsQuery(),
            cancellationToken));
    }

    [HttpGet("reviews/employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeReviews(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetEmployeeReviewsQuery(employeeId),
            cancellationToken));
    }

    [HttpPut("reviews/{reviewId:guid}/approve")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> ApproveReview(
        Guid reviewId,
        [FromBody] ApproveReviewRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new ApproveReviewCommand(reviewId, request.ReviewedByUserId),
            cancellationToken));
    }

    // KPIs endpoints
    [HttpPost("kpis")]
    public async Task<IActionResult> CreateKPI(
        CreateKPICommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetAllKPIs(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAllKPIsQuery(),
            cancellationToken));
    }

    [HttpGet("kpis/employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeKPIs(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetEmployeeKPIsQuery(employeeId),
            cancellationToken));
    }

    [HttpPut("kpis/{kpiId:guid}/achievement")]
    public async Task<IActionResult> UpdateKPIAchievement(
        Guid kpiId,
        [FromBody] UpdateKPIAchievementRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new UpdateKPIAchievementCommand(kpiId, request.Achieved),
            cancellationToken));
    }
}

/// <summary>Request models for performance endpoints</summary>
public sealed class UpdateGoalProgressRequest
{
    public int ProgressPercentage { get; set; }
}

public sealed class ApproveReviewRequest
{
    public Guid ReviewedByUserId { get; set; }
}

public sealed class UpdateKPIAchievementRequest
{
    public decimal Achieved { get; set; }
}
