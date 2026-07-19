using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance;

public sealed record ApplyLeaveCommand(
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason = null)
    : IRequest<LeaveRequestDto>;
