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
        var query = _context.LeaveRequests.AsNoTracking();

        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            query = query.Where(lr => lr.EmployeeId == request.EmployeeId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status) && request.Status != "All")
        {
            query = query.Where(lr => lr.Status == request.Status);
        }

        var list = await query.OrderByDescending(lr => lr.CreatedAtUtc).ToListAsync(cancellationToken);
        if (list.Count == 0)
        {
            return new List<LeaveRequestDto>();
        }

        var empIds = list.Select(l => l.EmployeeId).Distinct().ToList();
        var leaveTypeIds = list.Select(l => l.LeaveTypeId).Distinct().ToList();

        var employees = await _context.Employees
            .AsNoTracking()
            .Where(e => empIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim(), cancellationToken);

        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .Where(lt => leaveTypeIds.Contains(lt.Id))
            .ToDictionaryAsync(lt => lt.Id, lt => lt.Name, cancellationToken);

        return list.Select(leaveRequest => new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = employees.TryGetValue(leaveRequest.EmployeeId, out var name) ? name : "Employee",
            LeaveTypeId = leaveRequest.LeaveTypeId,
            LeaveTypeName = leaveTypes.TryGetValue(leaveRequest.LeaveTypeId, out var ltName) ? ltName : "Leave",
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
        }).ToList();
    }
}
