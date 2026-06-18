using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : DbContext,
      IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}