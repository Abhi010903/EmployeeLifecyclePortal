using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Attendance;

public sealed record GetLeaveTypesQuery
    : IRequest<List<LeaveTypeDto>>;
