using EmployeeLifecyclePortal.Domain.Entities;

namespace EmployeeLifecyclePortal.Application.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Department>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Department department,
        CancellationToken cancellationToken = default);

    void Remove(
        Department department);
}