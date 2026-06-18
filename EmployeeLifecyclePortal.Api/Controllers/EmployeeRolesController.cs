using EmployeeLifecyclePortal.Application.Commands.EmployeeRoles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmployeeRolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeeRolesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole(
        AssignRoleToEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(result);
    }
}