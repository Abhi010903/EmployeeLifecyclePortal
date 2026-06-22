using EmployeeLifecyclePortal.Application.Commands.Auth;
using EmployeeLifecyclePortal.Application.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>>
        Register(
            RegisterCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>>
        Login(
            LoginCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(result);
    }
}