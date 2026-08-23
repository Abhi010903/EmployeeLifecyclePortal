using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Attendance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Attendance.Handlers;

public sealed class GetLeaveBalanceQueryHandler
    : IRequestHandler<GetLeaveBalanceQuery, List<LeaveBalanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveBalanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveBalanceDto>> Handle(
        GetLeaveBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var balances = await _context.LeaveBalances
            .Where(lb => lb.EmployeeId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        var empName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : "Employee";
        var currentYear = DateTime.UtcNow.Year;

        if (balances.Count > 0)
        {
            return balances.Select(balance =>
            {
                var lt = leaveTypes.FirstOrDefault(t => t.Id == balance.LeaveTypeId);
                return new LeaveBalanceDto
                {
                    Id = balance.Id,
                    EmployeeId = balance.EmployeeId,
                    EmployeeName = empName,
                    LeaveTypeId = balance.LeaveTypeId,
                    LeaveTypeName = lt?.Name ?? "Leave",
                    TotalDays = balance.TotalDays,
                    UsedDays = balance.UsedDays,
                    RemainingDays = balance.RemainingDays,
                    Year = balance.Year,
                    CreatedAtUtc = balance.CreatedAtUtc,
                    CreatedBy = balance.CreatedBy,
                    LastModifiedAtUtc = balance.LastModifiedAtUtc,
                    LastModifiedBy = balance.LastModifiedBy
                };
            }).ToList();
        }

        // If no explicit balance records exist in table, calculate dynamically from master LeaveTypes and approved LeaveRequests
        var approvedLeaves = await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == request.EmployeeId && lr.Status == "Approved" && lr.StartDateUtc.Year == currentYear)
            .ToListAsync(cancellationToken);

        return leaveTypes.Select(lt =>
        {
            var used = approvedLeaves.Where(l => l.LeaveTypeId == lt.Id).Sum(l => l.GetDaysRequested());
            return new LeaveBalanceDto
            {
                Id = Guid.NewGuid(),
                EmployeeId = request.EmployeeId,
                EmployeeName = empName,
                LeaveTypeId = lt.Id,
                LeaveTypeName = lt.Name,
                TotalDays = lt.DaysPerYear,
                UsedDays = used,
                RemainingDays = Math.Max(0, lt.DaysPerYear - used),
                Year = currentYear,
                CreatedAtUtc = lt.CreatedAtUtc
            };
        }).ToList();
    }
}
