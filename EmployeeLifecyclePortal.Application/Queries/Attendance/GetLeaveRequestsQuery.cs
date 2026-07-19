using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Attendance;

public sealed record GetLeaveRequestsQuery(Guid? EmployeeId = null, string? Status = null)
    : IRequest<List<LeaveRequestDto>>;
