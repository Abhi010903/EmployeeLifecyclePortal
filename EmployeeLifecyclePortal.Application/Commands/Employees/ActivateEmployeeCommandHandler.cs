using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed class ActivateEmployeeCommandHandler
    : IRequestHandler<ActivateEmployeeCommand>
{
    private readonly IApplicationDbContext _context;

    public ActivateEmployeeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        ActivateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (employee is null)
            throw new InvalidOperationException(
                "Employee not found.");

        employee.Activate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}