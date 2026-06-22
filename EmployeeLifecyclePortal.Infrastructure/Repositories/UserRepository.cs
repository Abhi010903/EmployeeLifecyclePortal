using EmployeeLifecyclePortal.Application.Interfaces.Repositories.Auth;
using EmployeeLifecyclePortal.Domain.Entities.Auth;
using EmployeeLifecyclePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Infrastructure.Repositories;

public sealed class UserRepository
    : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return await _context.ApplicationUsers
            .FirstOrDefaultAsync(
                x => x.Email == email,
                cancellationToken);
    }

    public async Task AddAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        await _context.ApplicationUsers
            .AddAsync(user, cancellationToken);
    }
}