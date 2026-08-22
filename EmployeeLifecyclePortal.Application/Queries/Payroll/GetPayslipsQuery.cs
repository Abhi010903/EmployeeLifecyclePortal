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
        var departments = await _context.Departments.ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);
        var query = _context.Payslips.Include(p => p.Employee).AsQueryable();

        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            query = query.Where(p => p.EmployeeId == request.EmployeeId.Value);
        }

        var payslips = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);

        return payslips.Select(p =>
        {
            var deptName = p.Employee != null && departments.TryGetValue(p.Employee.DepartmentId, out var dname) ? dname : "General";
            return new PayslipDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeName = p.Employee?.FullName ?? "Unknown",
                EmployeeCode = p.Employee?.EmployeeCode ?? "",
                DepartmentName = deptName,
                Month = p.Month,
                Year = p.Year,
                BasicSalary = p.BasicSalary,
                Hra = p.Hra,
                Allowances = p.Allowances,
                BonusPay = p.BonusPay,
                OvertimePay = p.OvertimePay,
                GrossSalary = p.GrossSalary,
                PfDeduction = p.PfDeduction,
                EsiDeduction = p.EsiDeduction,
                TdsDeduction = p.TdsDeduction,
                Deductions = p.Deductions,
                ReimbursementsAmount = p.ReimbursementsAmount,
                NetSalary = p.NetSalary,
                WorkingDays = p.WorkingDays,
                PresentDays = p.PresentDays,
                PaidLeaveDays = p.PaidLeaveDays,
                UnpaidLeaveDays = p.UnpaidLeaveDays,
                Status = p.Status,
                PaymentMethod = p.PaymentMethod,
                PaymentDateUtc = p.PaymentDateUtc,
                Remarks = p.Remarks,
                GeneratedDateUtc = p.GeneratedDateUtc,
                CreatedAtUtc = p.CreatedAtUtc,
                CreatedBy = p.CreatedBy,
                LastModifiedAtUtc = p.LastModifiedAtUtc,
                LastModifiedBy = p.LastModifiedBy
            };
        }).ToList();
    }
}
