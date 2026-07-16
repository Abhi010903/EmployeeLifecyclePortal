namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>
/// Immutable audit log entry that records every create, update, and delete operation
/// on any auditable entity in the system.
///
/// Each audit log captures:
/// - What entity was modified and its ID
/// - Who performed the operation (user ID)
/// - When it was performed (timestamp in UTC)
/// - What operation (Create, Update, Delete)
/// - Which properties changed and their old/new values
/// </summary>
public sealed class AuditLog
{
    /// <summary>
    /// Unique identifier for this audit log entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Timestamp in UTC when the operation occurred.
    /// </summary>
    public DateTime OperatedAtUtc { get; private set; }

    /// <summary>
    /// User ID who performed the operation (typically from JWT claims or context).
    /// May be "system" for automatic operations.
    /// </summary>
    public string OperatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Type name of the entity that was modified (e.g., "Employee", "Department").
    /// </summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>
    /// Primary key value of the entity that was modified (stored as string for flexibility).
    /// </summary>
    public string EntityId { get; private set; } = string.Empty;

    /// <summary>
    /// The kind of operation: "Created", "Updated", or "Deleted".
    /// </summary>
    public string Operation { get; private set; } = string.Empty;

    /// <summary>
    /// JSON-serialized old values (before the change). Null for Create operations.
    /// Format: { "PropertyName": "OldValue", ... }
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// JSON-serialized new values (after the change). Null for Delete operations.
    /// Format: { "PropertyName": "NewValue", ... }
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// JSON-serialized list of property names that changed.
    /// Format: ["PropertyName1", "PropertyName2", ...]
    /// </summary>
    public string? ChangedColumns { get; private set; }

    private AuditLog()
    {
    }

    /// <summary>
    /// Creates a new audit log entry for a create operation.
    /// </summary>
    public static AuditLog CreateEntry(
        DateTime operatedAtUtc,
        string operatedBy,
        string entityType,
        string entityId,
        string operation,
        string? oldValues = null,
        string? newValues = null,
        string? changedColumns = null)
    {
        if (string.IsNullOrWhiteSpace(operatedBy))
            throw new ArgumentException("OperatedBy is required.", nameof(operatedBy));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType is required.", nameof(entityType));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId is required.", nameof(entityId));

        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("Operation is required.", nameof(operation));

        var validOperations = new[] { "Created", "Updated", "Deleted" };

        if (!validOperations.Contains(operation))
            throw new ArgumentException(
                $"Operation must be one of: {string.Join(", ", validOperations)}",
                nameof(operation));

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            OperatedAtUtc = operatedAtUtc,
            OperatedBy = operatedBy,
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedColumns = changedColumns
        };
    }
}
