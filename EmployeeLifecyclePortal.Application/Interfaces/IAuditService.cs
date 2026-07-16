using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EmployeeLifecyclePortal.Application.Interfaces;

/// <summary>
/// Service for tracking and recording audit logs for all entity modifications.
/// Converts EF Core change tracking into audit log entries.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Extracts audit logs from the current change tracker entries.
    /// Called by the DbContext's SaveChangesAsync before persisting changes.
    /// </summary>
    /// <param name="entries">The EF Core change tracker entries to audit.</param>
    /// <param name="userId">The user ID performing the operation (from JWT claims or context).</param>
    /// <returns>List of audit log entries to be persisted.</returns>
    List<AuditLog> GetAuditLogs(
        IEnumerable<EntityEntry> entries,
        string userId);
}
