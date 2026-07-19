using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;

namespace EmployeeLifecyclePortal.Application.Commands.Asset.Handlers;

public sealed class ScheduleMaintenanceCommandHandler : IRequestHandler<ScheduleMaintenanceCommand, AssetMaintenanceDto>
{
    private readonly IApplicationDbContext _context;

    public ScheduleMaintenanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetMaintenanceDto> Handle(
        ScheduleMaintenanceCommand request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.Assets.FindAsync(new object[] { request.AssetId }, cancellationToken: cancellationToken);
        if (asset == null)
            throw new InvalidOperationException("Asset not found");

        var maintenance = new AssetMaintenance(
            request.AssetId,
            request.Description,
            request.Cost,
            request.ServiceProvider);

        _context.AssetMaintenances.Add(maintenance);
        await _context.SaveChangesAsync(cancellationToken);

        return new AssetMaintenanceDto
        {
            Id = maintenance.Id,
            AssetId = maintenance.AssetId,
            AssetName = asset.AssetName,
            MaintenanceDateUtc = maintenance.MaintenanceDateUtc,
            Description = maintenance.Description,
            Cost = maintenance.Cost,
            ServiceProvider = maintenance.ServiceProvider,
            CreatedAtUtc = maintenance.CreatedAtUtc,
            CreatedBy = maintenance.CreatedBy
        };
    }
}
