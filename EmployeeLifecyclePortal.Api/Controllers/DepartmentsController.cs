using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Departments;
using EmployeeLifecyclePortal.Application.Queries.Departments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CreateDepartment(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> UpdateDepartment(
        Guid id,
        UpdateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command with { Id = id },
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Admin)]
    public async Task<IActionResult> DeleteDepartment(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteDepartmentCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDepartments(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAllDepartmentsQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDepartmentById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetDepartmentByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/employees")]
    public async Task<IActionResult> GetDepartmentEmployees(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetDepartmentEmployeesQuery(id),
            cancellationToken);

        return Ok(result);
    }
}