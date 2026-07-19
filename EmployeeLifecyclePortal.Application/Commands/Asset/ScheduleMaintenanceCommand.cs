using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.Commands.Asset;

public sealed class ScheduleMaintenanceCommand : IRequest<AssetMaintenanceDto>
{
    public Guid AssetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string ServiceProvider { get; set; } = string.Empty;

    public ScheduleMaintenanceCommand(Guid assetId, string description, decimal cost, string provider)
    {
        AssetId = assetId;
        Description = description;
        Cost = cost;
        ServiceProvider = provider;
    }
}
