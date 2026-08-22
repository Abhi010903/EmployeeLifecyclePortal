using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Payroll;

public sealed record ApprovePayrollRunCommand(int Month, int Year) : IRequest<bool>;

public sealed class ApprovePayrollRunCommandHandler : IRequestHandler<ApprovePayrollRunCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ApprovePayrollRunCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApprovePayrollRunCommand request, CancellationToken cancellationToken)
    {
        var payslips = await _context.Payslips
            .Where(p => p.Month == request.Month && p.Year == request.Year)
            .ToListAsync(cancellationToken);

        if (payslips.Count == 0)
            return false;

        foreach (var p in payslips)
        {
            p.MarkApproved();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
