using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Common;
using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace EmployeeLifecyclePortal.Infrastructure.Services;

/// <summary>
/// Implementation of audit logging that tracks all entity modifications.
/// Captures what changed, who changed it, when it happened, and the old/new values.
/// </summary>
public sealed class AuditService : IAuditService
{
    /// <summary>
    /// Extracts audit logs from EF Core change tracker.
    /// Processes Added, Modified, and Deleted entities to create audit trail entries.
    /// </summary>
    public List<AuditLog> GetAuditLogs(
        IEnumerable<EntityEntry> entries,
        string userId)
    {
        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            // Ignore audit log entries themselves to prevent recursive auditing
            if (entry.Entity is AuditLog)
            {
                continue;
            }

            // Only track auditable entities
            if (entry.Entity is not AuditableEntity)
            {
                continue;
            }

            // Skip entries that haven't actually changed
            if (entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Entity.GetType()
                .GetProperty("Id")
                ?.GetValue(entry.Entity)
                ?.ToString() ?? "unknown";

            switch (entry.State)
            {
                case EntityState.Added:
                    auditLogs.Add(CreateAuditLogForCreated(
                        entry,
                        entityType,
                        entityId,
                        userId));
                    break;

                case EntityState.Modified:
                    var modifiedLog = CreateAuditLogForModified(
                        entry,
                        entityType,
                        entityId,
                        userId);

                    if (modifiedLog is not null)
                    {
                        auditLogs.Add(modifiedLog);
                    }

                    break;

                case EntityState.Deleted:
                    auditLogs.Add(CreateAuditLogForDeleted(
                        entry,
                        entityType,
                        entityId,
                        userId));
                    break;
            }
        }

        return auditLogs;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static AuditLog CreateAuditLogForCreated(
        EntityEntry entry,
        string entityType,
        string entityId,
        string userId)
    {
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.CurrentValue is not null)
            {
                newValues[property.Metadata.Name] = property.CurrentValue;
            }
        }

        return AuditLog.CreateEntry(
            DateTime.UtcNow,
            userId,
            entityType,
            entityId,
            "Created",
            oldValues: null,
            newValues: SerializeValues(newValues),
            changedColumns: SerializePropertyNames(newValues.Keys.ToList()));
    }

    private static AuditLog? CreateAuditLogForModified(
        EntityEntry entry,
        string entityType,
        string entityId,
        string userId)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var changedProperties = new List<string>();

        foreach (var property in entry.Properties)
        {
            // Skip audit metadata fields (CreatedAt, CreatedBy, etc.)
            if (IsAuditMetadataProperty(property.Metadata.Name))
            {
                continue;
            }

            if (property.IsModified)
            {
                changedProperties.Add(property.Metadata.Name);
                oldValues[property.Metadata.Name] = property.OriginalValue;
                newValues[property.Metadata.Name] = property.CurrentValue;
            }
        }

        // Don't create audit log if only audit metadata changed
        if (changedProperties.Count == 0)
        {
            return null;
        }

        return AuditLog.CreateEntry(
            DateTime.UtcNow,
            userId,
            entityType,
            entityId,
            "Updated",
            oldValues: SerializeValues(oldValues),
            newValues: SerializeValues(newValues),
            changedColumns: SerializePropertyNames(changedProperties));
    }

    private static AuditLog CreateAuditLogForDeleted(
        EntityEntry entry,
        string entityType,
        string entityId,
        string userId)
    {
        var deletedValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.OriginalValue is not null)
            {
                deletedValues[property.Metadata.Name] = property.OriginalValue;
            }
        }

        return AuditLog.CreateEntry(
            DateTime.UtcNow,
            userId,
            entityType,
            entityId,
            "Deleted",
            oldValues: SerializeValues(deletedValues),
            newValues: null,
            changedColumns: SerializePropertyNames(deletedValues.Keys.ToList()));
    }

    private static string SerializeValues(Dictionary<string, object?> values)
    {
        try
        {
            return JsonSerializer.Serialize(values, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
        catch
        {
            // If JSON serialization fails, return a simple string representation
            return string.Join(", ", values.Select(kv => $"{kv.Key}: {kv.Value}"));
        }
    }

    private static string SerializePropertyNames(List<string> propertyNames)
    {
        return JsonSerializer.Serialize(propertyNames);
    }

    private static bool IsAuditMetadataProperty(string propertyName)
    {
        var auditMetadataProperties = new[]
        {
            nameof(AuditableEntity.CreatedAtUtc),
            nameof(AuditableEntity.CreatedBy),
            nameof(AuditableEntity.LastModifiedAtUtc),
            nameof(AuditableEntity.LastModifiedBy)
        };

        return auditMetadataProperties.Contains(propertyName);
    }
}
