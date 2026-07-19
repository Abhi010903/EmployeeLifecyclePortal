using MediatR;
using Microsoft.EntityFrameworkCore;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;

namespace EmployeeLifecyclePortal.Application.Commands.Asset.Handlers;

public sealed class AssignAssetCommandHandler : IRequestHandler<AssignAssetCommand, AssetAssignmentDto>
{
    private readonly IApplicationDbContext _context;

    public AssignAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetAssignmentDto> Handle(
        AssignAssetCommand request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.Assets.FindAsync(new object[] { request.AssetId }, cancellationToken: cancellationToken);
        if (asset == null)
            throw new InvalidOperationException("Asset not found");

        var assignment = new AssetAssignment(request.EmployeeId, request.AssetId);
        if (!string.IsNullOrEmpty(request.Notes))
        {
            // Set notes if needed through reflection or add a method to domain entity
        }

        asset.Assign();
        _context.AssetAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken: cancellationToken);

        return new AssetAssignmentDto
        {
            Id = assignment.Id,
            EmployeeId = assignment.EmployeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
            AssetId = assignment.AssetId,
            AssetName = asset.AssetName,
            AssetType = asset.AssetType,
            AssignedDateUtc = assignment.AssignedDateUtc,
            ReturnedDateUtc = assignment.ReturnedDateUtc,
            Status = assignment.Status,
            Notes = assignment.Notes,
            CreatedAtUtc = assignment.CreatedAtUtc,
            CreatedBy = assignment.CreatedBy
        };
    }
}
