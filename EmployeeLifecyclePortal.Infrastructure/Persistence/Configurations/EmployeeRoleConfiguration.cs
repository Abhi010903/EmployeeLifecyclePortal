using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLifecyclePortal.Infrastructure.Persistence.Configurations;

public sealed class EmployeeRoleConfiguration
    : IEntityTypeConfiguration<EmployeeRole>
{
    public void Configure(
        EntityTypeBuilder<EmployeeRole> builder)
    {
        builder.ToTable("EmployeeRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(
            x => new
            {
                x.EmployeeId,
                x.RoleId
            })
            .IsUnique();

        builder.HasOne<Employee>()
            .WithMany(x => x.EmployeeRoles)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Role>()
            .WithMany(x => x.EmployeeRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}