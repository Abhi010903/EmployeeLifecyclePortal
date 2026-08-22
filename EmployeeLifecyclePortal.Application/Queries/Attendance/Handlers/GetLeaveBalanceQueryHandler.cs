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
            .Include(lb => lb.Employee)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        if (balances.Count > 0)
        {
            return balances.Select(balance => new LeaveBalanceDto
            {
                Id = balance.Id,
                EmployeeId = balance.EmployeeId,
                EmployeeName = balance.Employee != null 
                    ? $"{balance.Employee.FirstName} {balance.Employee.LastName}".Trim()
                    : "Unknown",
                LeaveTypeId = balance.LeaveTypeId,
                LeaveTypeName = balance.LeaveType != null ? balance.LeaveType.Name : "Unknown",
                TotalDays = balance.TotalDays,
                UsedDays = balance.UsedDays,
                RemainingDays = balance.RemainingDays,
                Year = balance.Year,
                CreatedAtUtc = balance.CreatedAtUtc,
                CreatedBy = balance.CreatedBy,
                LastModifiedAtUtc = balance.LastModifiedAtUtc,
                LastModifiedBy = balance.LastModifiedBy
            }).ToList();
        }

        // If no explicit balance records, calculate dynamically from master LeaveTypes and approved LeaveRequests
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        var leaveTypes = await _context.LeaveTypes.ToListAsync(cancellationToken);
        var currentYear = DateTime.UtcNow.Year;
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
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : "Unknown",
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
