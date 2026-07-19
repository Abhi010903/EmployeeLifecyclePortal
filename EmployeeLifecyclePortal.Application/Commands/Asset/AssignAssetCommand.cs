using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.Commands.Asset;

public sealed class AssignAssetCommand : IRequest<AssetAssignmentDto>
{
    public Guid EmployeeId { get; set; }
    public Guid AssetId { get; set; }
    public string? Notes { get; set; }

    public AssignAssetCommand(Guid employeeId, Guid assetId, string? notes = null)
    {
        EmployeeId = employeeId;
        AssetId = assetId;
        Notes = notes;
    }
}
