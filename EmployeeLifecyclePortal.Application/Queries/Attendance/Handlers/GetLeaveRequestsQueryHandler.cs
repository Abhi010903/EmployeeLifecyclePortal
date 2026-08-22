using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Attendance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Attendance.Handlers;

public sealed class GetLeaveRequestsQueryHandler
    : IRequestHandler<GetLeaveRequestsQuery, List<LeaveRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveRequestDto>> Handle(
        GetLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsNoTracking();

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(lr => lr.EmployeeId == request.EmployeeId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status) && request.Status != "All")
        {
            query = query.Where(lr => lr.Status == request.Status);
        }

        return await query
            .Select(leaveRequest => new LeaveRequestDto
            {
                Id = leaveRequest.Id,
                EmployeeId = leaveRequest.EmployeeId,
                EmployeeName = leaveRequest.Employee != null 
                    ? $"{leaveRequest.Employee.FirstName} {leaveRequest.Employee.LastName}"
                    : "Unknown",
                LeaveTypeId = leaveRequest.LeaveTypeId,
                LeaveTypeName = leaveRequest.LeaveType != null ? leaveRequest.LeaveType.Name : "Unknown",
                StartDateUtc = leaveRequest.StartDateUtc,
                EndDateUtc = leaveRequest.EndDateUtc,
                DaysRequested = leaveRequest.GetDaysRequested(),
                Status = leaveRequest.Status,
                Reason = leaveRequest.Reason,
                ApprovedByUserId = leaveRequest.ApprovedByUserId,
                ManagerApprovedByUserId = leaveRequest.ManagerApprovedByUserId,
                ManagerApprovedAtUtc = leaveRequest.ManagerApprovedAtUtc,
                FinalApprovedByUserId = leaveRequest.FinalApprovedByUserId,
                FinalApprovedAtUtc = leaveRequest.FinalApprovedAtUtc,
                RejectedByUserId = leaveRequest.RejectedByUserId,
                RejectedAtUtc = leaveRequest.RejectedAtUtc,
                RejectionReason = leaveRequest.RejectionReason,
                CreatedAtUtc = leaveRequest.CreatedAtUtc,
                CreatedBy = leaveRequest.CreatedBy,
                LastModifiedAtUtc = leaveRequest.LastModifiedAtUtc,
                LastModifiedBy = leaveRequest.LastModifiedBy
            })
            .OrderByDescending(lr => lr.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
