using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Payroll;

public sealed record GetPayslipsByMonthQuery(int Month, int Year) : IRequest<List<PayslipDto>>;

public sealed class GetPayslipsByMonthQueryHandler : IRequestHandler<GetPayslipsByMonthQuery, List<PayslipDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPayslipsByMonthQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PayslipDto>> Handle(GetPayslipsByMonthQuery request, CancellationToken cancellationToken)
    {
        var payslips = await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.Month == request.Month && p.Year == request.Year)
            .OrderBy(p => p.Employee != null ? p.Employee.FirstName : "")
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
