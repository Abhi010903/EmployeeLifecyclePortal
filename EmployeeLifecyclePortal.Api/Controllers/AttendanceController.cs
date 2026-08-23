using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public AttendanceController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _context = context;
    }

    // Attendance endpoints
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn(
        CheckInCommand command,
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR";

        if (!isElevated || command.EmployeeId == Guid.Empty)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            command = command with { EmployeeId = empId };
        }

        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut(
        CheckOutCommand command,
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR";

        if (!isElevated || (!command.EmployeeId.HasValue || command.EmployeeId.Value == Guid.Empty))
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            command = command with { EmployeeId = empId };
        }

        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Manager)]
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
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        if (!isElevated)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            var result = await _mediator.Send(
                new GetAttendanceByEmployeeQuery(empId),
                cancellationToken);

            var todayUtc = DateTime.UtcNow.Date;
            var todayRecords = result.Where(r => r.CheckInTimeUtc.Date == todayUtc).ToList();
            return Ok(todayRecords);
        }

        return Ok(await _mediator.Send(
            new GetTodayAttendanceQuery(),
            cancellationToken));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAttendance(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        return Ok(await _mediator.Send(
            new GetAttendanceByEmployeeQuery(empId),
            cancellationToken));
    }

    [HttpGet("my/today")]
    public async Task<IActionResult> GetMyTodayAttendance(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var result = await _mediator.Send(
            new GetAttendanceByEmployeeQuery(empId),
            cancellationToken);

        var todayUtc = DateTime.UtcNow.Date;
        var todayRecords = result.Where(r => r.CheckInTimeUtc.Date == todayUtc).ToList();
        return Ok(todayRecords);
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetAttendanceByEmployee(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(employeeId, _context, cancellationToken))
        {
            return Forbid();
        }

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
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR";

        if (!isElevated || command.EmployeeId == Guid.Empty)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            command = command with { EmployeeId = empId };
        }

        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("leave/approve")]
    [Authorize(Policy = Permissions.Supervisor)]
    public async Task<IActionResult> ApproveLeave(
        ApproveLeaveCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("leave/reject")]
    [Authorize(Policy = Permissions.Supervisor)]
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

    [HttpGet("leave/balance/my")]
    public async Task<IActionResult> GetMyLeaveBalance(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        return Ok(await _mediator.Send(
            new GetLeaveBalanceQuery(empId),
            cancellationToken));
    }

    [HttpGet("leave/balance/{employeeId:guid}")]
    public async Task<IActionResult> GetLeaveBalance(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(employeeId, _context, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _mediator.Send(
            new GetLeaveBalanceQuery(employeeId),
            cancellationToken));
    }

    [HttpGet("leave/requests/my")]
    public async Task<IActionResult> GetMyLeaveRequests(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        return Ok(await _mediator.Send(
            new GetLeaveRequestsQuery(empId, status),
            cancellationToken));
    }

    [HttpGet("leave/requests")]
    public async Task<IActionResult> GetLeaveRequests(
        [FromQuery] Guid? employeeId,
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

        return Ok(await _mediator.Send(
            new GetLeaveRequestsQuery(employeeId, status),
            cancellationToken));
    }
}
