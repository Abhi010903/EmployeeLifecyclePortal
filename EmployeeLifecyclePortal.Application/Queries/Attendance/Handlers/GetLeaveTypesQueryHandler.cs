using EmployeeLifecyclePortal.Application.DTOs.Attendance;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Attendance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Attendance.Handlers;

public sealed class GetLeaveTypesQueryHandler
    : IRequestHandler<GetLeaveTypesQuery, List<LeaveTypeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveTypesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveTypeDto>> Handle(
        GetLeaveTypesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.LeaveTypes
            .Select(leaveType => new LeaveTypeDto
            {
                Id = leaveType.Id,
                Name = leaveType.Name,
                DaysPerYear = leaveType.DaysPerYear,
                IsPaid = leaveType.IsPaid,
                Description = leaveType.Description,
                CreatedAtUtc = leaveType.CreatedAtUtc,
                CreatedBy = leaveType.CreatedBy,
                LastModifiedAtUtc = leaveType.LastModifiedAtUtc,
                LastModifiedBy = leaveType.LastModifiedBy
            })
            .ToListAsync(cancellationToken);
    }
}
