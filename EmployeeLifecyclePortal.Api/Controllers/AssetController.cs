using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Asset;
using EmployeeLifecyclePortal.Application.DTOs.Asset;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Asset;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/assets")]
[Authorize(Policy = Permissions.Employee)]
public sealed class AssetController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AssetController(
        IMediator mediator,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _context = context;
        _currentUserService = currentUserService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAssets(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        return Ok(await _mediator.Send(
            new GetEmployeeAssetsQuery(empId),
            cancellationToken));
    }

    [HttpGet("assignments")]
    public async Task<IActionResult> GetAllAssignments(
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        var query = _context.AssetAssignments
            .Include(aa => aa.Employee)
            .Include(aa => aa.Asset)
            .AsQueryable();

        if (!isElevated)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            query = query.Where(aa => aa.EmployeeId == empId);
        }

        var assignments = await query
            .OrderByDescending(aa => aa.AssignedDateUtc)
            .Select(aa => new AssetAssignmentDto
            {
                Id = aa.Id,
                EmployeeId = aa.EmployeeId,
                EmployeeName = aa.Employee != null ? $"{aa.Employee.FirstName} {aa.Employee.LastName}" : "Unknown",
                AssetId = aa.AssetId,
                AssetName = aa.Asset != null ? aa.Asset.AssetName : "Unknown",
                AssetType = aa.Asset != null ? aa.Asset.AssetType : "Unknown",
                AssignedDateUtc = aa.AssignedDateUtc,
                ReturnedDateUtc = aa.ReturnedDateUtc,
                Status = aa.Status,
                Notes = aa.Notes,
                CreatedAtUtc = aa.CreatedAtUtc,
                CreatedBy = aa.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return Ok(assignments);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CreateAsset(
        CreateAssetCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAssets(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAllAssetsQuery(),
            cancellationToken));
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableAssets(
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAvailableAssetsQuery(),
            cancellationToken));
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeAssets(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(employeeId, _context, cancellationToken))
        {
            return Forbid();
        }

        return Ok(await _mediator.Send(
            new GetEmployeeAssetsQuery(employeeId),
            cancellationToken));
    }

    [HttpGet("{assetId:guid}/history")]
    public async Task<IActionResult> GetAssetHistory(
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetAssetHistoryQuery(assetId),
            cancellationToken));
    }

    [HttpPost("{assetId:guid}/assign")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> AssignAsset(
        Guid assetId,
        [FromBody] AssignAssetRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new AssignAssetCommand(request.EmployeeId, assetId, request.Notes),
            cancellationToken));
    }

    [HttpPost("{assignmentId:guid}/return")]
    public async Task<IActionResult> ReturnAsset(
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new ReturnAssetCommand(assignmentId),
            cancellationToken));
    }

    [HttpGet("{assetId:guid}/maintenance")]
    public async Task<IActionResult> GetMaintenanceHistory(
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetMaintenanceHistoryQuery(assetId),
            cancellationToken));
    }

    [HttpPost("{assetId:guid}/maintenance")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> ScheduleMaintenance(
        Guid assetId,
        [FromBody] ScheduleMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new ScheduleMaintenanceCommand(assetId, request.Description, request.Cost, request.ServiceProvider),
            cancellationToken));
    }
}

public sealed class AssignAssetRequest
{
    public Guid EmployeeId { get; set; }
    public string? Notes { get; set; }
}

public sealed class ScheduleMaintenanceRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string ServiceProvider { get; set; } = string.Empty;
}
