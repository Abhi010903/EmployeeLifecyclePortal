using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Attendance.Handlers;

public sealed class CheckInCommandHandler
    : IRequestHandler<CheckInCommand, AttendanceDto>
{
    private readonly IApplicationDbContext _context;

    public CheckInCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceDto> Handle(
        CheckInCommand request,
        CancellationToken cancellationToken)
    {
        var attendance = Domain.Entities.Attendance.CreateCheckIn(request.EmployeeId);

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        return new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = employee != null 
                ? $"{employee.FirstName} {employee.LastName}"
                : "Unknown",
            CheckInTimeUtc = attendance.CheckInTimeUtc,
            CheckOutTimeUtc = attendance.CheckOutTimeUtc,
            Status = attendance.Status,
            IsApproved = attendance.IsApproved,
            Notes = request.Notes,
            HoursWorked = attendance.GetHoursWorked(),
            CreatedAtUtc = attendance.CreatedAtUtc,
            CreatedBy = attendance.CreatedBy,
            LastModifiedAtUtc = attendance.LastModifiedAtUtc,
            LastModifiedBy = attendance.LastModifiedBy
        };
    }
}
