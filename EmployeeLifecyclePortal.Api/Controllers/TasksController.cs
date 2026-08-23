using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Tasks;
using EmployeeLifecyclePortal.Application.DTOs.Tasks;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public TasksController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _context = context;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var result = await _mediator.Send(
            new GetWorkTasksQuery(empId, null, status),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? departmentId,
        [FromQuery] string? status,
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
            employeeId = empId;
        }

        var result = await _mediator.Send(
            new GetWorkTasksQuery(employeeId, departmentId, status),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Supervisor)]
    public async Task<IActionResult> CreateTask(
        CreateWorkTaskCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateWorkTaskStatusCommand command,
        CancellationToken cancellationToken)
    {
        var task = await _context.WorkTasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task == null)
            return NotFound("Task not found.");

        if (!await _currentUserService.HasAccessToEmployeeAsync(task.EmployeeId, _context, cancellationToken))
        {
            return Forbid();
        }

        var result = await _mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(result);
    }
}
