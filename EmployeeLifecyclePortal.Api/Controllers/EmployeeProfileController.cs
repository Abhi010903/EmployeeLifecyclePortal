using EmployeeLifecyclePortal.Application.Commands.Employees;
using EmployeeLifecyclePortal.Application.DTOs;
using EmployeeLifecyclePortal.Application.Queries.Employees;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Employee")]
public sealed class EmployeeProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmployeeProfileController> _logger;

    public EmployeeProfileController(
        IMediator mediator,
        ILogger<EmployeeProfileController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get complete employee profile including roles, manager, and activity counts.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching employee profile — Employee: {EmployeeId}", id);

        var query = new GetEmployeeProfileQuery(EmployeeId: id);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get employee's timeline events (promotions, transfers, training, etc.)
    /// </summary>
    [HttpGet("{id}/timeline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimeline(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching employee timeline — Employee: {EmployeeId}", id);

        var query = new GetEmployeeTimelineQuery(EmployeeId: id);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Assign a manager to an employee (Manager/HR/Admin only)
    /// </summary>
    [HttpPost("{id}/manager")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignManager(
        Guid id,
        [FromBody] AssignManagerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ManagerId == Guid.Empty)
            return BadRequest("Manager ID is required.");

        _logger.LogInformation(
            "Assigning manager — Employee: {EmployeeId} | Manager: {ManagerId}",
            id,
            request.ManagerId);

        var command = new AssignManagerCommand(
            EmployeeId: id,
            ManagerId: request.ManagerId);

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Remove manager assignment from an employee (Manager/HR/Admin only)
    /// </summary>
    [HttpDelete("{id}/manager")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveManager(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing manager — Employee: {EmployeeId}", id);

        var command = new AssignManagerCommand(
            EmployeeId: id,
            ManagerId: Guid.Empty);

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}

public sealed class AssignManagerRequest
{
    public Guid ManagerId { get; set; }
}
