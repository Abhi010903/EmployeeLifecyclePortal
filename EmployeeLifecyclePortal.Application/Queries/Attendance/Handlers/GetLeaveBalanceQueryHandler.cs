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
        return await _context.LeaveBalances
            .Include(lb => lb.Employee)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == request.EmployeeId)
            .Select(balance => new LeaveBalanceDto
            {
                Id = balance.Id,
                EmployeeId = balance.EmployeeId,
                EmployeeName = balance.Employee != null 
                    ? $"{balance.Employee.FirstName} {balance.Employee.LastName}"
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
            })
            .ToListAsync(cancellationToken);
    }
}
