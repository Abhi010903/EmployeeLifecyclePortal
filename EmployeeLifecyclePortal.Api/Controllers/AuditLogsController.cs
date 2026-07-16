using EmployeeLifecyclePortal.Application.Queries.AuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

/// <summary>
/// API endpoints for viewing audit logs and entity change history.
/// Restricted to Admin users only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Admin")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IMediator mediator,
        ILogger<AuditLogsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated audit logs with optional filtering by entity type and/or operation.
    /// </summary>
    /// <param name="entityType">Optional: Filter by entity type (e.g., "Employee", "Department")</param>
    /// <param name="operation">Optional: Filter by operation ("Created", "Updated", "Deleted")</param>
    /// <param name="pageNumber">Page number for pagination (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 50, max: 250)</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType = null,
        [FromQuery] string? operation = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 250) pageSize = 250;

        _logger.LogInformation(
            "Fetching audit logs — EntityType: {EntityType} | Operation: {Operation} | Page: {PageNumber}/{PageSize}",
            entityType ?? "All",
            operation ?? "All",
            pageNumber,
            pageSize);

        var query = new GetAuditLogsQuery(
            EntityType: entityType,
            Operation: operation,
            PageNumber: pageNumber,
            PageSize: pageSize);

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves the complete modification history of a specific entity.
    /// Shows all changes to one entity from creation to last modification.
    /// </summary>
    /// <param name="entityType">Type of the entity (e.g., "Employee", "Department")</param>
    /// <param name="entityId">Primary key of the entity</param>
    [HttpGet("history/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEntityHistory(
        [FromRoute] string entityType,
        [FromRoute] string entityId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return BadRequest("Entity type is required.");

        if (string.IsNullOrWhiteSpace(entityId))
            return BadRequest("Entity ID is required.");

        _logger.LogInformation(
            "Fetching entity history — EntityType: {EntityType} | EntityId: {EntityId}",
            entityType,
            entityId);

        var query = new GetEntityHistoryQuery(
            EntityType: entityType,
            EntityId: entityId);

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
