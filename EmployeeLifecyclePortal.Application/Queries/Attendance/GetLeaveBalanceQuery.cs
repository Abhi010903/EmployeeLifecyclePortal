using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Attendance;

public sealed record GetLeaveBalanceQuery(Guid EmployeeId)
    : IRequest<List<LeaveBalanceDto>>;
