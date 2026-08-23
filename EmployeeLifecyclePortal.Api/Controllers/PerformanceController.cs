using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Performance;
using EmployeeLifecyclePortal.Application.Interfaces;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public PerformanceController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _context = context;
    }

    // Goals endpoints
    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal(
        CreateGoalCommand command,
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        if (!isElevated || command.EmployeeId == Guid.Empty)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            command = command with { EmployeeId = empId };
        }

        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("goals/my")]
    public async Task<IActionResult> GetMyGoals(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        return Ok(await _mediator.Send(
            new GetEmployeeGoalsQuery(empId),
            cancellationToken));
    }

    [HttpGet("goals")]
    public async Task<IActionResult> GetAllGoals(
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        if (!isElevated)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            return Ok(await _mediator.Send(
                new GetEmployeeGoalsQuery(empId),
                cancellationToken));
        }

        return Ok(await _mediator.Send(
            new GetAllGoalsQuery(),
            cancellationToken));
    }

    [HttpGet("goals/employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeGoals(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(employeeId, _context, cancellationToken))
        {
            return Forbid();
        }

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

    [HttpGet("reviews/my")]
    public async Task<IActionResult> GetMyReviews(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        return Ok(await _mediator.Send(
            new GetEmployeeReviewsQuery(empId),
            cancellationToken));
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> GetAllReviews(
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        if (!isElevated)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            return Ok(await _mediator.Send(
                new GetEmployeeReviewsQuery(empId),
                cancellationToken));
        }

        return Ok(await _mediator.Send(
            new GetAllReviewsQuery(),
            cancellationToken));
    }

    [HttpGet("reviews/employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeReviews(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(employeeId, _context, cancellationToken))
        {
            return Forbid();
        }

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

    [HttpGet("kpis/my")]
    public async Task<IActionResult> GetMyKPIs(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        return Ok(await _mediator.Send(
            new GetEmployeeKPIsQuery(empId),
            cancellationToken));
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetAllKPIs(
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        if (!isElevated)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            return Ok(await _mediator.Send(
                new GetEmployeeKPIsQuery(empId),
                cancellationToken));
        }

        return Ok(await _mediator.Send(
            new GetAllKPIsQuery(),
            cancellationToken));
    }

    [HttpGet("kpis/employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeKPIs(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(employeeId, _context, cancellationToken))
        {
            return Forbid();
        }

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
