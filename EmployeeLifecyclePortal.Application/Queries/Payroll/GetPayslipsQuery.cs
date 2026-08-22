using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Payroll;

public sealed record GetPayslipsQuery(Guid? EmployeeId = null) : IRequest<List<PayslipDto>>;

public sealed class GetPayslipsQueryHandler : IRequestHandler<GetPayslipsQuery, List<PayslipDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPayslipsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PayslipDto>> Handle(GetPayslipsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payslips.Include(p => p.Employee).AsQueryable();

        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            query = query.Where(p => p.EmployeeId == request.EmployeeId.Value);
        }

        var payslips = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);

        return payslips.Select(p => new PayslipDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            EmployeeName = p.Employee?.FullName ?? "Unknown",
            Month = p.Month,
            Year = p.Year,
            GrossSalary = p.GrossSalary,
            Deductions = p.Deductions,
            NetSalary = p.NetSalary,
            Status = p.Status,
            GeneratedDateUtc = p.GeneratedDateUtc,
            CreatedAtUtc = p.CreatedAtUtc,
            CreatedBy = p.CreatedBy,
            LastModifiedAtUtc = p.LastModifiedAtUtc,
            LastModifiedBy = p.LastModifiedBy
        }).ToList();
    }
}
