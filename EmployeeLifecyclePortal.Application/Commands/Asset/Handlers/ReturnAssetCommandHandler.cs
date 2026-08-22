using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;

namespace EmployeeLifecyclePortal.Application.Commands.Asset.Handlers;

public sealed class ReturnAssetCommandHandler : IRequestHandler<ReturnAssetCommand, AssetAssignmentDto>
{
    private readonly IApplicationDbContext _context;

    public ReturnAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetAssignmentDto> Handle(
        ReturnAssetCommand request,
        CancellationToken cancellationToken)
    {
        var assignment = await _context.AssetAssignments
            .Include(aa => aa.Employee)
            .Include(aa => aa.Asset)
            .FirstOrDefaultAsync(aa => aa.Id == request.AssignmentId, cancellationToken);

        if (assignment == null)
            throw new InvalidOperationException("Assignment not found");

        var asset = assignment.Asset;
        if (asset != null)
        {
            asset.Status = "Available";
        }

        assignment.Return();
        await _context.SaveChangesAsync(cancellationToken);

        return new AssetAssignmentDto
        {
            Id = assignment.Id,
            EmployeeId = assignment.EmployeeId,
            EmployeeName = assignment.Employee != null ? $"{assignment.Employee.FirstName} {assignment.Employee.LastName}" : "Unknown",
            AssetId = assignment.AssetId,
            AssetName = asset?.AssetName ?? "Unknown",
            AssetType = asset?.AssetType ?? "Unknown",
            AssignedDateUtc = assignment.AssignedDateUtc,
            ReturnedDateUtc = assignment.ReturnedDateUtc,
            Status = assignment.Status,
            Notes = assignment.Notes,
            CreatedAtUtc = assignment.CreatedAtUtc,
            CreatedBy = assignment.CreatedBy
        };
    }
}
