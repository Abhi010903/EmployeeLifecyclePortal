using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance;

public sealed record CheckInCommand(Guid EmployeeId, string? Notes = null)
    : IRequest<AttendanceDto>;
