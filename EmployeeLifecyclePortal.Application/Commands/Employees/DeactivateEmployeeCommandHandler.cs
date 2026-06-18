using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed class DeactivateEmployeeCommandHandler
    : IRequestHandler<DeactivateEmployeeCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateEmployeeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        DeactivateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (employee is null)
            throw new InvalidOperationException(
                "Employee not found.");

        employee.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}