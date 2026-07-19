using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.Queries.Asset;

public sealed class GetMaintenanceHistoryQuery : IRequest<IEnumerable<AssetMaintenanceDto>>
{
    public Guid AssetId { get; }

    public GetMaintenanceHistoryQuery(Guid assetId)
    {
        AssetId = assetId;
    }
}
