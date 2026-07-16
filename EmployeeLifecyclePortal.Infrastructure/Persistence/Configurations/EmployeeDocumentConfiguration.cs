using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLifecyclePortal.Infrastructure.Persistence.Configurations;

public sealed class EmployeeDocumentConfiguration
    : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FilePath)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.FileType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileSizeBytes)
            .IsRequired();

        builder.Property(x => x.ExpirationDateUtc)
            .HasColumnType("datetime2");

        builder.Property(x => x.Notes)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        // ── Indexes for common queries ────────────────────────────────────
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.DocumentType);
        builder.HasIndex(x => x.ExpirationDateUtc);
        builder.HasIndex(x => x.IsArchived);
        builder.HasIndex(x => new { x.EmployeeId, x.DocumentType });
        builder.HasIndex(x => new { x.ExpirationDateUtc, x.IsArchived });

        // ── Foreign key relationship ──────────────────────────────────────
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
