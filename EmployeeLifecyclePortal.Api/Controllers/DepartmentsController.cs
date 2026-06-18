using EmployeeLifecyclePortal.Application.Commands.Departments;
using EmployeeLifecyclePortal.Application.Queries.Departments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(result);
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