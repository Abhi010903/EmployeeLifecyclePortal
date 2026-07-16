namespace EmployeeLifecyclePortal.Application.DTOs;

/// <summary>
/// Data transfer object for audit log records.
/// Contains the full audit trail for a single entity modification.
/// </summary>
public sealed class AuditLogDto
{
    /// <summary>
    /// Unique identifier for this audit log entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp in UTC when the operation occurred.
    /// </summary>
    public DateTime OperatedAtUtc { get; set; }

    /// <summary>
    /// User ID who performed the operation.
    /// </summary>
    public string OperatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Type name of the entity that was modified (e.g., "Employee", "Department").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Primary key value of the entity that was modified.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The kind of operation: "Created", "Updated", or "Deleted".
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Old values before the change (JSON).
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// New values after the change (JSON).
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// List of property names that changed (JSON array).
    /// </summary>
    public string? ChangedColumns { get; set; }

    /// <summary>
    /// Friendly display of when the change occurred (e.g., "5 minutes ago").
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var elapsed = DateTime.UtcNow - OperatedAtUtc;

            return elapsed.TotalSeconds < 60
                ? $"{(int)elapsed.TotalSeconds} seconds ago"
                : elapsed.TotalMinutes < 60
                    ? $"{(int)elapsed.TotalMinutes} minutes ago"
                    : elapsed.TotalHours < 24
                        ? $"{(int)elapsed.TotalHours} hours ago"
                        : $"{(int)elapsed.TotalDays} days ago";
        }
    }
}

/// <summary>
/// DTO for displaying entity modification history.
/// Shows all changes to a specific entity grouped chronologically.
/// </summary>
public sealed class EntityHistoryDto
{
    /// <summary>
    /// Type name of the entity (e.g., "Employee").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Primary key of the entity.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// All audit log entries for this entity, ordered from newest to oldest.
    /// </summary>
    public List<AuditLogDto> Modifications { get; set; } = [];

    /// <summary>
    /// Total number of modifications recorded for this entity.
    /// </summary>
    public int TotalModifications => Modifications.Count;

    /// <summary>
    /// When the entity was first created.
    /// </summary>
    public DateTime CreatedAtUtc => Modifications
        .OrderBy(x => x.OperatedAtUtc)
        .FirstOrDefault()
        ?.OperatedAtUtc ?? DateTime.UtcNow;

    /// <summary>
    /// When the entity was last modified.
    /// </summary>
    public DateTime LastModifiedAtUtc => Modifications
        .OrderByDescending(x => x.OperatedAtUtc)
        .FirstOrDefault()
        ?.OperatedAtUtc ?? DateTime.UtcNow;
}
