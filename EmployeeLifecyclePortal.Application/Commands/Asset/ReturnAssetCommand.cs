using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.Commands.Asset;

public sealed class ReturnAssetCommand : IRequest<AssetAssignmentDto>
{
    public Guid AssignmentId { get; set; }

    public ReturnAssetCommand(Guid assignmentId)
    {
        AssignmentId = assignmentId;
    }
}
