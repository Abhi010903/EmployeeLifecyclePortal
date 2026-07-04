using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Employees;
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

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
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
    public async Task<IActionResult> GetAllEmployees(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAllEmployeesQuery(),
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEmployeeById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetEmployeeByIdQuery(id),
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
        return Ok(await _mediator.Send(
            new GetEmployeeRolesQuery(id),
            cancellationToken));
    }
}