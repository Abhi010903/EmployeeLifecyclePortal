using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance.Handlers;

public sealed class CheckOutCommandHandler
    : IRequestHandler<CheckOutCommand, AttendanceDto>
{
    private readonly IApplicationDbContext _context;

    public CheckOutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceDto> Handle(
        CheckOutCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Attendance? attendance = null;

        if (request.AttendanceId.HasValue && request.AttendanceId.Value != Guid.Empty)
        {
            attendance = await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == request.AttendanceId.Value, cancellationToken);
        }

        if (attendance is null && request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            var empId = request.EmployeeId.Value;
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == empId, cancellationToken);
            if (emp == null)
            {
                var appUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == empId, cancellationToken);
                if (appUser != null)
                {
                    emp = await _context.Employees.FirstOrDefaultAsync(e => e.Email == appUser.Email, cancellationToken);
                }
            }

            if (emp != null)
            {
                attendance = await _context.Attendances
                    .Include(a => a.Employee)
                    .Where(a => a.EmployeeId == emp.Id && a.CheckOutTimeUtc == null)
                    .OrderByDescending(a => a.CheckInTimeUtc)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (attendance is null)
        {
            throw new InvalidOperationException("No active attendance session found for this employee.");
        }

        if (attendance.CheckOutTimeUtc.HasValue)
        {
            throw new InvalidOperationException("This attendance session has already been checked out.");
        }

        attendance.CheckOut();
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        var employeeName = attendance.Employee != null 
            ? $"{attendance.Employee.FirstName} {attendance.Employee.LastName}".Trim()
            : "Unknown";

        return new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = employeeName,
            CheckInTimeUtc = attendance.CheckInTimeUtc,
            CheckOutTimeUtc = attendance.CheckOutTimeUtc,
            Status = attendance.Status,
            IsApproved = attendance.IsApproved,
            Notes = attendance.Notes,
            HoursWorked = attendance.GetHoursWorked(),
            CreatedAtUtc = attendance.CreatedAtUtc,
            CreatedBy = attendance.CreatedBy,
            LastModifiedAtUtc = attendance.LastModifiedAtUtc,
            LastModifiedBy = attendance.LastModifiedBy
        };
    }
}
