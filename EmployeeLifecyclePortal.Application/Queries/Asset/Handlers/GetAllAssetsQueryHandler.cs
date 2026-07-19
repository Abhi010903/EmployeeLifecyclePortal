using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;

namespace EmployeeLifecyclePortal.Application.Queries.Asset.Handlers;

public sealed class GetAllAssetsQueryHandler : IRequestHandler<GetAllAssetsQuery, IEnumerable<AssetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllAssetsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetDto>> Handle(
        GetAllAssetsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Assets
            .Select(a => new AssetDto
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                AssetType = a.AssetType,
                SerialNumber = a.SerialNumber,
                PurchaseValue = a.PurchaseValue,
                Status = a.Status,
                Condition = a.Condition,
                PurchaseDateUtc = a.PurchaseDateUtc,
                CreatedAtUtc = a.CreatedAtUtc,
                CreatedBy = a.CreatedBy,
                LastModifiedAtUtc = a.LastModifiedAtUtc,
                LastModifiedBy = a.LastModifiedBy
            })
            .ToListAsync(cancellationToken);
    }
}
