using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Payroll;

public sealed record GetPayrollSummaryQuery(int Month, int Year) : IRequest<PayrollSummaryDto>;

public sealed class GetPayrollSummaryQueryHandler : IRequestHandler<GetPayrollSummaryQuery, PayrollSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetPayrollSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollSummaryDto> Handle(GetPayrollSummaryQuery request, CancellationToken cancellationToken)
    {
        var employees = await _context.Employees.ToListAsync(cancellationToken);
        var departments = await _context.Departments.ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);

        var payslips = await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.Month == request.Month && p.Year == request.Year)
            .ToListAsync(cancellationToken);

        var deptBreakdown = employees
            .GroupBy(e => e.DepartmentId)
            .Select(g =>
            {
                var deptName = departments.TryGetValue(g.Key, out var name) ? name : "General";
                var empIds = g.Select(e => e.Id).ToHashSet();
                var deptPayslips = payslips.Where(p => empIds.Contains(p.EmployeeId)).ToList();
                return new DepartmentPayrollDto
                {
                    DepartmentName = deptName,
                    EmployeeCount = g.Count(),
                    TotalGross = deptPayslips.Sum(p => p.GrossSalary),
                    TotalNet = deptPayslips.Sum(p => p.NetSalary)
                };
            }).ToList();

        var payslipDtos = payslips.Select(p =>
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
                CreatedAtUtc = p.CreatedAtUtc
            };
        }).ToList();

        var status = payslips.Count > 0 ? payslips.First().Status : "Draft";

        return new PayrollSummaryDto
        {
            Month = request.Month,
            Year = request.Year,
            TotalEmployees = employees.Count,
            ProcessedCount = payslips.Count,
            TotalGrossSalary = payslips.Sum(p => p.GrossSalary),
            TotalDeductions = payslips.Sum(p => p.Deductions),
            TotalPf = payslips.Sum(p => p.PfDeduction),
            TotalEsi = payslips.Sum(p => p.EsiDeduction),
            TotalTds = payslips.Sum(p => p.TdsDeduction),
            TotalReimbursements = payslips.Sum(p => p.ReimbursementsAmount),
            TotalNetSalary = payslips.Sum(p => p.NetSalary),
            Status = status,
            DepartmentBreakdown = deptBreakdown,
            Payslips = payslipDtos
        };
    }
}
