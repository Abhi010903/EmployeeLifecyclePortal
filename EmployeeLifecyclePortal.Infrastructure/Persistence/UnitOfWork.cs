using EmployeeLifecyclePortal.Application.Interfaces;

namespace EmployeeLifecyclePortal.Infrastructure.Persistence;

public sealed class UnitOfWork
    : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(
            cancellationToken);
    }
}