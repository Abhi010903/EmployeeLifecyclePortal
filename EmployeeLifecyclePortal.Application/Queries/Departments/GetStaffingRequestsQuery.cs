using EmployeeLifecyclePortal.Application.DTOs.Departments;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Departments;

public sealed record GetStaffingRequestsQuery(
    Guid? DepartmentId = null)
    : IRequest<List<StaffingRequestDto>>;

public sealed class GetStaffingRequestsQueryHandler : IRequestHandler<GetStaffingRequestsQuery, List<StaffingRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffingRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StaffingRequestDto>> Handle(GetStaffingRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StaffingRequests
            .Include(sr => sr.Department)
            .AsNoTracking();

        if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
        {
            query = query.Where(sr => sr.DepartmentId == request.DepartmentId.Value);
        }

        var list = await query
            .OrderByDescending(sr => sr.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return list.Select(sr => new StaffingRequestDto
        {
            Id = sr.Id,
            DepartmentId = sr.DepartmentId,
            DepartmentName = sr.Department?.Name ?? "Unknown",
            RequestedByUserId = sr.RequestedByUserId,
            RequestedByName = "Manager",
            CurrentHeadcount = sr.CurrentHeadcount,
            RequiredCount = sr.RequiredCount,
            Reason = sr.Reason,
            Status = sr.Status,
            AdminComments = sr.AdminComments,
            CreatedAtUtc = sr.CreatedAtUtc,
            ResolvedAtUtc = sr.ResolvedAtUtc
        }).ToList();
    }
}
