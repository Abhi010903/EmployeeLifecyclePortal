using EmployeeLifecyclePortal.Application.Commands.Employees;
using EmployeeLifecyclePortal.Application.Queries.Employees;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEmployee(
        Guid id,
        UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command with { Id = id },
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/activate")]
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
    public async Task<IActionResult> TerminateEmployee(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new TerminateEmployeeCommand(id),
            cancellationToken);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees(
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
        var result = await _mediator.Send(
            new GetEmployeeByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/roles")]
    public async Task<IActionResult> GetEmployeeRoles(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetEmployeeRolesQuery(id),
            cancellationToken);

        return Ok(result);
    }
}