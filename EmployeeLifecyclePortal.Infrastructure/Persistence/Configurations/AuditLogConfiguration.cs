using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLifecyclePortal.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for AuditLog.
/// Defines the database schema and constraints for audit log records.
/// </summary>
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.OperatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(x => x.OperatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.EntityId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Operation)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OldValues)
            .IsRequired(false)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.NewValues)
            .IsRequired(false)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ChangedColumns)
            .IsRequired(false)
            .HasColumnType("nvarchar(max)");

        // ── Indexes for common queries ────────────────────────────────────────
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.OperatedAtUtc);
        builder.HasIndex(x => x.OperatedBy);
        builder.HasIndex(x => x.Operation);

        builder.ToTable("AuditLogs", "dbo");
    }
}
