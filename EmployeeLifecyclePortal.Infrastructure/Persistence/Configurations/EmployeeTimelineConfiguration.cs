using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLifecyclePortal.Infrastructure.Persistence.Configurations;

public sealed class EmployeeTimelineConfiguration
    : IEntityTypeConfiguration<EmployeeTimeline>
{
    public void Configure(EntityTypeBuilder<EmployeeTimeline> builder)
    {
        builder.ToTable("EmployeeTimelines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.EventDateUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(x => x.Category)
            .HasMaxLength(255);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        // ── Indexes for common queries ────────────────────────────────────
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.EventDateUtc);
        builder.HasIndex(x => new { x.EmployeeId, x.EventDateUtc });

        // ── Foreign key relationship ──────────────────────────────────────
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.Timelines)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
