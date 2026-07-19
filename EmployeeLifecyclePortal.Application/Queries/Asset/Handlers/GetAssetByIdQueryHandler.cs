using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Asset.Handlers;

public sealed class GetAssetByIdQueryHandler
    : IRequestHandler<GetAssetByIdQuery, AssetDto>
{
    private readonly IApplicationDbContext _context;

    public GetAssetByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetDto> Handle(
        GetAssetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

        if (asset == null)
        {
            throw new KeyNotFoundException($"Asset with ID {request.AssetId} not found");
        }

        return new AssetDto
        {
            Id = asset.Id,
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName,
            AssetType = asset.AssetType,
            SerialNumber = asset.SerialNumber,
            PurchaseValue = asset.PurchaseValue,
            Status = asset.Status,
            Condition = asset.Condition,
            PurchaseDateUtc = asset.PurchaseDateUtc,
            CreatedAtUtc = asset.CreatedAtUtc,
            CreatedBy = asset.CreatedBy,
            LastModifiedAtUtc = asset.LastModifiedAtUtc,
            LastModifiedBy = asset.LastModifiedBy
        };
    }
}
