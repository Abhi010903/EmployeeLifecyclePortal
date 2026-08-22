using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance;

public sealed record CheckOutCommand(Guid? AttendanceId = null, Guid? EmployeeId = null)
    : IRequest<AttendanceDto>;
