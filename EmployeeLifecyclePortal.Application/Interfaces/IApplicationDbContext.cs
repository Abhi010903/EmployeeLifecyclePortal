using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Employee> Employees { get; }

    DbSet<Department> Departments { get; }

    DbSet<Role> Roles { get; }

    DbSet<EmployeeRole> EmployeeRoles { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}