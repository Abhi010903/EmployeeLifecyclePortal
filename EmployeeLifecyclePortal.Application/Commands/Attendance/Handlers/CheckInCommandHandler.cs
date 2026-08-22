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
        if (request.EmployeeId == Guid.Empty)
        {
            throw new InvalidOperationException("Employee ID is required for check-in.");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            var appUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == request.EmployeeId, cancellationToken);
            if (appUser != null)
            {
                employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == appUser.Email, cancellationToken);
            }
        }

        if (employee is null)
        {
            throw new InvalidOperationException($"Employee with ID {request.EmployeeId} was not found.");
        }

        var todayStartUtc = DateTime.UtcNow.Date;
        var todayEndUtc = todayStartUtc.AddDays(1);

        var todaySessions = await _context.Attendances
            .Where(a => a.EmployeeId == employee.Id && a.CheckInTimeUtc >= todayStartUtc && a.CheckInTimeUtc < todayEndUtc)
            .ToListAsync(cancellationToken);

        // Validate that the employee does NOT have an active unclosed session
        if (todaySessions.Any(a => a.CheckOutTimeUtc == null))
        {
            throw new InvalidOperationException($"Employee {employee.FullName} already has an active check-in session. Please check out before checking in again.");
        }

        // Determine role session limits (Part J: Manager/Team Lead = 2, Employee = 1)
        var isManagerOrLead = await _context.EmployeeRoles
            .Where(er => er.EmployeeId == employee.Id)
            .Join(_context.Roles, er => er.RoleId, r => r.Id, (er, r) => r.Name)
            .AnyAsync(rName => rName == "Manager" || rName == "Team Lead" || rName == "TeamLead" || rName == "Admin", cancellationToken);

        var maxSessions = isManagerOrLead ? 2 : 1;
        if (todaySessions.Count >= maxSessions)
        {
            var roleDescription = isManagerOrLead ? "Managers and Team Leads (maximum 2 sessions per day)" : "Employees (maximum 1 session per day)";
            throw new InvalidOperationException($"Daily check-in limit reached for {employee.FullName} under business rule for {roleDescription}.");
        }

        var attendance = Domain.Entities.Attendance.CreateCheckIn(employee.Id);

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return new AttendanceDto
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = employee.FullName,
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
