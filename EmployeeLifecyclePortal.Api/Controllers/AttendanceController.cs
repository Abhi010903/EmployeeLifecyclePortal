using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Attendance;
using EmployeeLifecyclePortal.Application.Queries.Attendance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Attendance endpoints
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn(
        CheckInCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut(
        CheckOutCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAttendance(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAllAttendanceQuery(),
            cancellationToken));
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodayAttendance(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetTodayAttendanceQuery(),
            cancellationToken));
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetAttendanceByEmployee(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAttendanceByEmployeeQuery(employeeId),
            cancellationToken));
    }

    // Leave endpoints
    [HttpPost("leave/apply")]
    public async Task<IActionResult> ApplyLeave(
        ApplyLeaveCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("leave/approve")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> ApproveLeave(
        ApproveLeaveCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("leave/reject")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> RejectLeave(
        RejectLeaveCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("leave/types")]
    public async Task<IActionResult> GetLeaveTypes(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetLeaveTypesQuery(),
            cancellationToken));
    }

    [HttpGet("leave/balance/{employeeId:guid}")]
    public async Task<IActionResult> GetLeaveBalance(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetLeaveBalanceQuery(employeeId),
            cancellationToken));
    }

    [HttpGet("leave/requests")]
    public async Task<IActionResult> GetLeaveRequests(
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetLeaveRequestsQuery(employeeId, status),
            cancellationToken));
    }
}
