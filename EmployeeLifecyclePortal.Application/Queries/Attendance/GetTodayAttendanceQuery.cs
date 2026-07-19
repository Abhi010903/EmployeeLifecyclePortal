using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Attendance;

public sealed record GetTodayAttendanceQuery
    : IRequest<List<AttendanceDto>>;
