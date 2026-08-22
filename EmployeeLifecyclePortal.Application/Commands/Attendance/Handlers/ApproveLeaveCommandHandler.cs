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

        if (leaveRequest.EmployeeId == request.ApprovedByUserId)
        {
            throw new InvalidOperationException("Users are not permitted to self-approve their own leave requests.");
        }

        // Get approver user to check role
        var approverUser = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == request.ApprovedByUserId, cancellationToken);

        var approverRole = approverUser?.Role ?? "Admin";

        if ((approverRole == "Manager" || approverRole == "Team Lead" || approverRole == "TeamLead") && leaveRequest.Status == "Pending")
        {
            // Manager stage approval -> moves to ManagerApproved (pending final Admin approval)
            leaveRequest.ManagerApprove(request.ApprovedByUserId);
        }
        else
        {
            // Admin final approval (or direct Admin approval) -> Approved
            leaveRequest.FinalApprove(request.ApprovedByUserId);

            // Update leave balance upon final approval
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
            ManagerApprovedByUserId = leaveRequest.ManagerApprovedByUserId,
            ManagerApprovedAtUtc = leaveRequest.ManagerApprovedAtUtc,
            FinalApprovedByUserId = leaveRequest.FinalApprovedByUserId,
            FinalApprovedAtUtc = leaveRequest.FinalApprovedAtUtc,
            CreatedAtUtc = leaveRequest.CreatedAtUtc,
            CreatedBy = leaveRequest.CreatedBy,
            LastModifiedAtUtc = leaveRequest.LastModifiedAtUtc,
            LastModifiedBy = leaveRequest.LastModifiedBy
        };
    }
}
