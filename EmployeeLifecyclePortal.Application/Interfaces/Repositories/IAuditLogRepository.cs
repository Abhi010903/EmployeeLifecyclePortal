using EmployeeLifecyclePortal.Domain.Entities;

namespace EmployeeLifecyclePortal.Application.Interfaces.Repositories;

/// <summary>
/// Repository for querying audit logs.
/// Provides read-only access to the complete audit trail.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets all audit logs for a specific entity (identified by type and ID).
    /// Results are ordered from newest to oldest.
    /// </summary>
    Task<List<AuditLog>> GetAuditLogsForEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all audit logs, optionally filtered by entity type and/or operation.
    /// Results are paginated and ordered from newest to oldest.
    /// </summary>
    Task<(List<AuditLog> Items, int TotalCount)> GetAuditLogsAsync(
        string? entityType = null,
        string? operation = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all audit logs for a specific user (who performed the operations).
    /// </summary>
    Task<List<AuditLog>> GetAuditLogsByUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs within a date range.
    /// </summary>
    Task<List<AuditLog>> GetAuditLogsByDateRangeAsync(
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of audit logs for a specific entity.
    /// Useful for determining if an entity has been modified.
    /// </summary>
    Task<int> GetAuditLogCountForEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);
}
