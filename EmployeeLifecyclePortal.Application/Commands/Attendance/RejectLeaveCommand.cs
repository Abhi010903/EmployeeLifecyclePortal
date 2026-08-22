using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance;

public sealed record RejectLeaveCommand(
    Guid LeaveRequestId,
    Guid? RejectedByUserId = null,
    string? Reason = null)
    : IRequest<LeaveRequestDto>;
