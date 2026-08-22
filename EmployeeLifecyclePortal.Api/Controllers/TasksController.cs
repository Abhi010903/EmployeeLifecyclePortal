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

    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Guid? employeeId,
        [FromQuery] Guid? departmentId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;
        var userEmail = _currentUserService.Email;

        if (role == "Employee" && !string.IsNullOrEmpty(userEmail))
        {
            var emp = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail, cancellationToken);
            if (emp != null)
            {
                employeeId = emp.Id;
            }
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
        var result = await _mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(result);
    }
}
