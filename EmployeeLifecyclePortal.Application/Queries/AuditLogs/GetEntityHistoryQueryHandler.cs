using EmployeeLifecyclePortal.Application.DTOs;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Application.Queries.AuditLogs;

/// <summary>
/// Handles retrieval of a specific entity's complete modification history.
/// Returns all changes ordered chronologically from creation to last modification.
/// </summary>
public sealed class GetEntityHistoryQueryHandler
    : IRequestHandler<GetEntityHistoryQuery, EntityHistoryDto>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<GetEntityHistoryQueryHandler> _logger;

    public GetEntityHistoryQueryHandler(
        IAuditLogRepository auditLogRepository,
        ILogger<GetEntityHistoryQueryHandler> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<EntityHistoryDto> Handle(
        GetEntityHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType))
            throw new ArgumentException("EntityType is required.", nameof(request.EntityType));

        if (string.IsNullOrWhiteSpace(request.EntityId))
            throw new ArgumentException("EntityId is required.", nameof(request.EntityId));

        _logger.LogInformation(
            "Retrieving entity history — EntityType: {EntityType} | EntityId: {EntityId}",
            request.EntityType,
            request.EntityId);

        var auditLogs = await _auditLogRepository.GetAuditLogsForEntityAsync(
            entityType: request.EntityType,
            entityId: request.EntityId,
            cancellationToken: cancellationToken);

        var auditLogDtos = auditLogs
            .OrderByDescending(x => x.OperatedAtUtc)
            .Select(x => MapToDto(x))
            .ToList();

        _logger.LogInformation(
            "Entity history retrieved — EntityType: {EntityType} | EntityId: {EntityId} | Total Modifications: {Count}",
            request.EntityType,
            request.EntityId,
            auditLogDtos.Count);

        return new EntityHistoryDto
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Modifications = auditLogDtos
        };
    }

    private static AuditLogDto MapToDto(Domain.Entities.AuditLog auditLog)
    {
        return new AuditLogDto
        {
            Id = auditLog.Id,
            OperatedAtUtc = auditLog.OperatedAtUtc,
            OperatedBy = auditLog.OperatedBy,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            Operation = auditLog.Operation,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues,
            ChangedColumns = auditLog.ChangedColumns
        };
    }
}
