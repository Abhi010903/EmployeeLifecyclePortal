using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Employees;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Employees;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public EmployeesController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentEmployeeProfile(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var result = await _mediator.Send(
            new GetEmployeeProfileQuery(empId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("me/timeline")]
    public async Task<IActionResult> GetCurrentEmployeeTimeline(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var result = await _mediator.Send(
            new GetEmployeeTimelineQuery(empId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CreateEmployee(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> GetAllEmployees(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAllEmployeesQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEmployeeById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(id, _context, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _mediator.Send(
            new GetEmployeeByIdQuery(id),
            cancellationToken));
    }

    [HttpGet("{id:guid}/profile")]
    public async Task<IActionResult> GetEmployeeProfile(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(id, _context, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _mediator.Send(
            new GetEmployeeProfileQuery(id),
            cancellationToken));
    }

    [HttpGet("{id:guid}/timeline")]
    public async Task<IActionResult> GetEmployeeTimeline(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(id, _context, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _mediator.Send(
            new GetEmployeeTimelineQuery(id),
            cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> UpdateEmployee(
        Guid id,
        UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            command with { Id = id },
            cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Admin)]
    public async Task<IActionResult> DeleteEmployee(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteEmployeeCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> ActivateEmployee(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ActivateEmployeeCommand(id),
            cancellationToken);

        return Ok();
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> DeactivateEmployee(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeactivateEmployeeCommand(id),
            cancellationToken);

        return Ok();
    }

    [HttpPost("{id:guid}/terminate")]
    [Authorize(Policy = Permissions.Admin)]
    public async Task<IActionResult> TerminateEmployee(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new TerminateEmployeeCommand(id),
            cancellationToken);

        return Ok();
    }

    [HttpGet("{id:guid}/roles")]
    public async Task<IActionResult> GetEmployeeRoles(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(id, _context, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _mediator.Send(
            new GetEmployeeRolesQuery(id),
            cancellationToken));
    }
}