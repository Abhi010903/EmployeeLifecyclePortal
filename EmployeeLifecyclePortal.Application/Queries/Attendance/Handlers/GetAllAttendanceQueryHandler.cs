using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Attendance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Attendance.Handlers;

public sealed class GetAllAttendanceQueryHandler
    : IRequestHandler<GetAllAttendanceQuery, List<AttendanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllAttendanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AttendanceDto>> Handle(
        GetAllAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Attendances
            .Include(a => a.Employee)
            .Select(attendance => new AttendanceDto
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
            })
            .OrderByDescending(a => a.CheckInTimeUtc)
            .ToListAsync(cancellationToken);
    }
}
