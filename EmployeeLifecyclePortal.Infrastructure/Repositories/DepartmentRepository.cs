using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Infrastructure.Repositories;

public sealed class DepartmentRepository
    : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<List<Department>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Department department,
        CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(
            department,
            cancellationToken);
    }

    public void Remove(
        Department department)
    {
        _context.Departments.Remove(department);
    }
}