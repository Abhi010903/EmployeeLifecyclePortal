using EmployeeLifecyclePortal.Application.DTOs;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Application.Queries.AuditLogs;

/// <summary>
/// Handles retrieval of audit logs with filtering and pagination.
/// </summary>
public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, GetAuditLogsResponse>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<GetAuditLogsQueryHandler> _logger;

    public GetAuditLogsQueryHandler(
        IAuditLogRepository auditLogRepository,
        ILogger<GetAuditLogsQueryHandler> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<GetAuditLogsResponse> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving audit logs — EntityType: {EntityType} | Operation: {Operation} | Page: {PageNumber}/{PageSize}",
            request.EntityType ?? "All",
            request.Operation ?? "All",
            request.PageNumber,
            request.PageSize);

        var (items, totalCount) = await _auditLogRepository.GetAuditLogsAsync(
            entityType: request.EntityType,
            operation: request.Operation,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var auditLogDtos = items.Select(x => MapToDto(x)).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new GetAuditLogsResponse(
            Items: auditLogDtos,
            TotalCount: totalCount,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalPages: totalPages);
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
