using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed class TerminateEmployeeCommandHandler
    : IRequestHandler<TerminateEmployeeCommand>
{
    private readonly IApplicationDbContext _context;

    public TerminateEmployeeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        TerminateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (employee is null)
            throw new InvalidOperationException(
                "Employee not found.");

        employee.Terminate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}