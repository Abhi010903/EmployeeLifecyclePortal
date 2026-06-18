using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed class DeleteDepartmentCommandHandler
    : IRequestHandler<DeleteDepartmentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteDepartmentCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (department is null)
        {
            throw new InvalidOperationException(
                "Department not found.");
        }

        bool hasEmployees =
            await _context.Employees.AnyAsync(
                x => x.DepartmentId == request.Id,
                cancellationToken);

        if (hasEmployees)
        {
            throw new InvalidOperationException(
                "Cannot delete department with assigned employees.");
        }

        _context.Departments.Remove(department);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}