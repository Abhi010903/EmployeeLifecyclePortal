using EmployeeLifecyclePortal.Domain.Entities;

namespace EmployeeLifecyclePortal.Application.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Employee>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default);

    void Update(
        Employee employee);

    void Remove(
        Employee employee);
}