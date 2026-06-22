using EmployeeLifecyclePortal.Domain.Entities.Auth;

namespace EmployeeLifecyclePortal.Application.Interfaces.Repositories.Auth;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task AddAsync(
        ApplicationUser user,
        CancellationToken cancellationToken);
}