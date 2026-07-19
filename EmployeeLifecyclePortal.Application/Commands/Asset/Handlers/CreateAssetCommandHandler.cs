using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;

namespace EmployeeLifecyclePortal.Application.Commands.Asset.Handlers;

public sealed class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, AssetDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetDto> Handle(
        CreateAssetCommand request,
        CancellationToken cancellationToken)
    {
        var asset = new Domain.Entities.Asset(
            request.AssetCode,
            request.AssetName,
            request.AssetType,
            request.SerialNumber,
            request.PurchaseValue,
            request.PurchaseDateUtc);

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

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
            CreatedBy = asset.CreatedBy
        };
    }
}
