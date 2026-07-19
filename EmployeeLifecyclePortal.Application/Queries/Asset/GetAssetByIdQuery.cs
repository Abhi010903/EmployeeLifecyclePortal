using EmployeeLifecyclePortal.Application.DTOs.Asset;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Queries.Asset;

public sealed record GetAssetByIdQuery(Guid AssetId)
    : IRequest<AssetDto>;
