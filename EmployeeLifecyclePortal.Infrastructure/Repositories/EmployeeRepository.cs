using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Infrastructure.Repositories;

public sealed class EmployeeRepository
    : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<List<Employee>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(
            employee,
            cancellationToken);
    }

    public void Update(
        Employee employee)
    {
        _context.Employees.Update(employee);
    }

    public void Remove(
        Employee employee)
    {
        _context.Employees.Remove(employee);
    }
}