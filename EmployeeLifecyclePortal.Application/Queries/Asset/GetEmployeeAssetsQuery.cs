using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.Queries.Asset;

public sealed class GetEmployeeAssetsQuery : IRequest<IEnumerable<AssetAssignmentDto>>
{
    public Guid EmployeeId { get; }

    public GetEmployeeAssetsQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}
