using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.Queries.Asset;

public sealed class GetAssetHistoryQuery : IRequest<IEnumerable<AssetAssignmentDto>>
{
    public Guid AssetId { get; }

    public GetAssetHistoryQuery(Guid assetId)
    {
        AssetId = assetId;
    }
}
