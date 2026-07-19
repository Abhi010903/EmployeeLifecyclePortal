using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance.Handlers;

public sealed class ApplyLeaveCommandHandler
    : IRequestHandler<ApplyLeaveCommand, LeaveRequestDto>
{
    private readonly IApplicationDbContext _context;

    public ApplyLeaveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequestDto> Handle(
        ApplyLeaveCommand request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = LeaveRequest.CreateRequest(
            request.EmployeeId,
            request.LeaveTypeId,
            request.StartDate,
            request.EndDate,
            request.Reason ?? string.Empty);

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        var leaveType = await _context.LeaveTypes
            .FirstOrDefaultAsync(lt => lt.Id == request.LeaveTypeId, cancellationToken);

        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = employee != null 
                ? $"{employee.FirstName} {employee.LastName}"
                : "Unknown",
            LeaveTypeId = leaveRequest.LeaveTypeId,
            LeaveTypeName = leaveType != null ? leaveType.Name : "Unknown",
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
