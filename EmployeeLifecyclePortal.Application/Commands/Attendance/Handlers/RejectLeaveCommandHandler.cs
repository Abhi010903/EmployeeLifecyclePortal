using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance.Handlers;

public sealed class RejectLeaveCommandHandler
    : IRequestHandler<RejectLeaveCommand, LeaveRequestDto>
{
    private readonly IApplicationDbContext _context;

    public RejectLeaveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequestDto> Handle(
        RejectLeaveCommand request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"Leave request with ID {request.LeaveRequestId} not found");

        if (request.RejectedByUserId.HasValue && leaveRequest.EmployeeId == request.RejectedByUserId.Value)
        {
            throw new InvalidOperationException("Users cannot reject their own leave requests through the approval workflow.");
        }

        leaveRequest.Reject(request.RejectedByUserId, request.Reason);
        _context.LeaveRequests.Update(leaveRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return new LeaveRequestDto
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
            RejectedByUserId = leaveRequest.RejectedByUserId,
            RejectedAtUtc = leaveRequest.RejectedAtUtc,
            RejectionReason = leaveRequest.RejectionReason,
            CreatedAtUtc = leaveRequest.CreatedAtUtc,
            CreatedBy = leaveRequest.CreatedBy,
            LastModifiedAtUtc = leaveRequest.LastModifiedAtUtc,
            LastModifiedBy = leaveRequest.LastModifiedBy
        };
    }
}
