using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Asset;
using EmployeeLifecyclePortal.Application.Queries.Asset;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class AssetController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssetController(IMediator mediator)
    {
        _mediator = mediator;
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
