using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Infrastructure.Repositories;

public sealed class RoleRepository
    : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<List<Role>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(
            role,
            cancellationToken);
    }

    public void Remove(
        Role role)
    {
        _context.Roles.Remove(role);
    }
}