using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance.Handlers;

public sealed class ApproveLeaveCommandHandler
    : IRequestHandler<ApproveLeaveCommand, LeaveRequestDto>
{
    private readonly IApplicationDbContext _context;

    public ApproveLeaveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequestDto> Handle(
        ApproveLeaveCommand request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"Leave request with ID {request.LeaveRequestId} not found");

        leaveRequest.Approve(request.ApprovedByUserId);

        // Update leave balance
        var leaveBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(
                lb => lb.EmployeeId == leaveRequest.EmployeeId && 
                      lb.LeaveTypeId == leaveRequest.LeaveTypeId,
                cancellationToken);

        if (leaveBalance != null)
        {
            leaveBalance.UseLeave(leaveRequest.GetDaysRequested());
            _context.LeaveBalances.Update(leaveBalance);
        }

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
            CreatedAtUtc = leaveRequest.CreatedAtUtc,
            CreatedBy = leaveRequest.CreatedBy,
            LastModifiedAtUtc = leaveRequest.LastModifiedAtUtc,
            LastModifiedBy = leaveRequest.LastModifiedBy
        };
    }
}
