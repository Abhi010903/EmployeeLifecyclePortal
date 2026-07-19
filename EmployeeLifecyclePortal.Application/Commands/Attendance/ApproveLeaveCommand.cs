using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance;

public sealed record ApproveLeaveCommand(
    Guid LeaveRequestId,
    Guid ApprovedByUserId)
    : IRequest<LeaveRequestDto>;
