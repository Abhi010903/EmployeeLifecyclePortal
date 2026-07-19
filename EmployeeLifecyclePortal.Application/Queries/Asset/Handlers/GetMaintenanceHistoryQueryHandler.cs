using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;

namespace EmployeeLifecyclePortal.Application.Queries.Asset.Handlers;

public sealed class GetMaintenanceHistoryQueryHandler : IRequestHandler<GetMaintenanceHistoryQuery, IEnumerable<AssetMaintenanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMaintenanceHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetMaintenanceDto>> Handle(
        GetMaintenanceHistoryQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.AssetMaintenances
            .Where(am => am.AssetId == request.AssetId)
            .Include(am => am.Asset)
            .OrderByDescending(am => am.MaintenanceDateUtc)
            .Select(am => new AssetMaintenanceDto
            {
                Id = am.Id,
                AssetId = am.AssetId,
                AssetName = am.Asset != null ? am.Asset.AssetName : "Unknown",
                MaintenanceDateUtc = am.MaintenanceDateUtc,
                Description = am.Description,
                Cost = am.Cost,
                ServiceProvider = am.ServiceProvider,
                CreatedAtUtc = am.CreatedAtUtc,
                CreatedBy = am.CreatedBy,
                LastModifiedAtUtc = am.LastModifiedAtUtc,
                LastModifiedBy = am.LastModifiedBy
            })
            .ToListAsync(cancellationToken);
    }
}
