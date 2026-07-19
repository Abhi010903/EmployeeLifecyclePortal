using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;

namespace EmployeeLifecyclePortal.Application.Queries.Asset.Handlers;

public sealed class GetAssetHistoryQueryHandler : IRequestHandler<GetAssetHistoryQuery, IEnumerable<AssetAssignmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssetHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetAssignmentDto>> Handle(
        GetAssetHistoryQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.AssetAssignments
            .Where(aa => aa.AssetId == request.AssetId)
            .Include(aa => aa.Employee)
            .Include(aa => aa.Asset)
            .OrderByDescending(aa => aa.AssignedDateUtc)
            .Select(aa => new AssetAssignmentDto
            {
                Id = aa.Id,
                EmployeeId = aa.EmployeeId,
                EmployeeName = aa.Employee != null ? $"{aa.Employee.FirstName} {aa.Employee.LastName}" : "Unknown",
                AssetId = aa.AssetId,
                AssetName = aa.Asset != null ? aa.Asset.AssetName : "Unknown",
                AssetType = aa.Asset != null ? aa.Asset.AssetType : "Unknown",
                AssignedDateUtc = aa.AssignedDateUtc,
                ReturnedDateUtc = aa.ReturnedDateUtc,
                Status = aa.Status,
                Notes = aa.Notes,
                CreatedAtUtc = aa.CreatedAtUtc,
                CreatedBy = aa.CreatedBy,
                LastModifiedAtUtc = aa.LastModifiedAtUtc,
                LastModifiedBy = aa.LastModifiedBy
            })
            .ToListAsync(cancellationToken);
    }
}
