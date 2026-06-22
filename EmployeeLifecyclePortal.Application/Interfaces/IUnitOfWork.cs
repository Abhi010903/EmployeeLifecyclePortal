namespace EmployeeLifecyclePortal.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync(
        CancellationToken cancellationToken = default);
}