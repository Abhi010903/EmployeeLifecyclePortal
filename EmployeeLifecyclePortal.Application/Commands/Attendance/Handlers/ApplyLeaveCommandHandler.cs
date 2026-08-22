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
        if (request.EmployeeId == Guid.Empty)
        {
            throw new InvalidOperationException("Employee ID is required.");
        }

        if (request.LeaveTypeId == Guid.Empty)
        {
            throw new InvalidOperationException("Leave Type is required.");
        }

        if (request.EndDate < request.StartDate)
        {
            throw new InvalidOperationException("End date cannot be earlier than start date.");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            // Check if request.EmployeeId is ApplicationUser.Id and match by email
            var appUser = await _context.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Id == request.EmployeeId, cancellationToken);
            if (appUser != null)
            {
                employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == appUser.Email, cancellationToken);
            }
        }

        if (employee is null)
        {
            throw new InvalidOperationException($"Employee with ID {request.EmployeeId} was not found.");
        }

        var leaveType = await _context.LeaveTypes
            .FirstOrDefaultAsync(lt => lt.Id == request.LeaveTypeId, cancellationToken);

        if (leaveType is null)
        {
            throw new InvalidOperationException($"Leave type with ID {request.LeaveTypeId} was not found.");
        }

        var leaveRequest = LeaveRequest.CreateRequest(
            employee.Id,
            leaveType.Id,
            request.StartDate,
            request.EndDate,
            request.Reason ?? string.Empty);

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            LeaveTypeId = leaveRequest.LeaveTypeId,
            LeaveTypeName = leaveType.Name,
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
