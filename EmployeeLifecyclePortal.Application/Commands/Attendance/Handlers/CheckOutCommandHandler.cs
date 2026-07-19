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
        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == request.AttendanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Attendance record with ID {request.AttendanceId} not found");

        attendance.CheckOut();
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = attendance.Employee != null 
                ? $"{attendance.Employee.FirstName} {attendance.Employee.LastName}"
                : "Unknown",
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
