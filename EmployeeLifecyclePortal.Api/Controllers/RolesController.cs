using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Roles;
using EmployeeLifecyclePortal.Application.Queries.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Admin)]
    public async Task<IActionResult> CreateRole(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Admin)]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command with { Id = id },
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Admin)]
    public async Task<IActionResult> DeleteRole(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteRoleCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAllRolesQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoleById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetRoleByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }
}