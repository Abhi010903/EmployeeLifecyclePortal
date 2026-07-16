using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Infrastructure.Repositories;

/// <summary>
/// Implementation of audit log repository.
/// Provides read-only access to the audit trail with various filtering options.
/// </summary>
public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuditLog>> GetAuditLogsForEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.OperatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<AuditLog> Items, int TotalCount)> GetAuditLogsAsync(
        string? entityType = null,
        string? operation = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(x => x.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(operation))
        {
            query = query.Where(x => x.Operation == operation);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.OperatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<AuditLog>> GetAuditLogsByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(x => x.OperatedBy == userId)
            .OrderByDescending(x => x.OperatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetAuditLogsByDateRangeAsync(
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(x => x.OperatedAtUtc >= startDateUtc && x.OperatedAtUtc <= endDateUtc)
            .OrderByDescending(x => x.OperatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAuditLogCountForEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .CountAsync(cancellationToken);
    }
}
