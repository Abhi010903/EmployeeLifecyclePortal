using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.Queries.Asset;

public sealed class GetAllAssetsQuery : IRequest<IEnumerable<AssetDto>>
{
}
