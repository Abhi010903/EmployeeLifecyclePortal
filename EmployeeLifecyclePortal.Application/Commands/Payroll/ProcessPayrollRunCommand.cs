using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Payroll;

public sealed record ProcessPayrollRunCommand(int Month, int Year) : IRequest<PayrollSummaryDto>;

public sealed class ProcessPayrollRunCommandHandler : IRequestHandler<ProcessPayrollRunCommand, PayrollSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public ProcessPayrollRunCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollSummaryDto> Handle(ProcessPayrollRunCommand request, CancellationToken cancellationToken)
    {
        var employees = await _context.Employees
            .Include(e => e.EmployeeRoles)
            .ToListAsync(cancellationToken);

        var startDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var endDate = startDate.AddDays(daysInMonth);

        // Fetch salary structures
        var salaryStructures = await _context.SalaryStructures
            .Where(s => s.IsActive)
            .ToDictionaryAsync(s => s.EmployeeId, cancellationToken);

        // Fetch attendance sessions for this month
        var attendances = await _context.Attendances
            .Where(a => a.CheckInTimeUtc >= startDate && a.CheckInTimeUtc < endDate)
            .ToListAsync(cancellationToken);

        var attendanceByEmployee = attendances
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Fetch approved leave requests for this month
        var leaveRequests = await _context.LeaveRequests
            .Include(l => l.LeaveType)
            .Where(l => l.Status == "Approved" && l.StartDateUtc < endDate && l.EndDateUtc >= startDate)
            .ToListAsync(cancellationToken);

        var leavesByEmployee = leaveRequests
            .GroupBy(l => l.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Fetch approved reimbursements pending payment or for this month
        var periodString = $"{request.Year}-{request.Month:D2}";
        var approvedReimbursements = await _context.Reimbursements
            .Where(r => r.Status == "Approved" && (r.PayrollPeriod == null || r.PayrollPeriod == periodString))
            .ToListAsync(cancellationToken);

        var reimbursementsByEmployee = approvedReimbursements
            .GroupBy(r => r.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Existing payslips for this month
        var existingPayslips = await _context.Payslips
            .Where(p => p.Month == request.Month && p.Year == request.Year)
            .ToDictionaryAsync(p => p.EmployeeId, cancellationToken);

        var generatedPayslips = new List<Payslip>();

        foreach (var employee in employees)
        {
            salaryStructures.TryGetValue(employee.Id, out var structure);
            decimal baseSalary = structure?.BaseSalary ?? 65000m;
            decimal hra = structure?.Hra > 0 ? structure.Hra : Math.Round(baseSalary * 0.40m, 2);
            decimal allowances = structure?.SpecialAllowance > 0 ? structure.SpecialAllowance : Math.Round(baseSalary * 0.15m, 2);

            // Attendance calculation
            attendanceByEmployee.TryGetValue(employee.Id, out var empAttendances);
            int presentDays = empAttendances?.Select(a => a.CheckInTimeUtc.Date).Distinct().Count() ?? 22;
            if (presentDays == 0) presentDays = 22; // default standard working days if testing fresh period

            // Leave calculation
            leavesByEmployee.TryGetValue(employee.Id, out var empLeaves);
            int paidLeaveDays = empLeaves?.Where(l => l.LeaveType?.IsPaid == true).Sum(l => l.GetDaysRequested()) ?? 0;
            int unpaidLeaveDays = empLeaves?.Where(l => l.LeaveType?.IsPaid == false).Sum(l => l.GetDaysRequested()) ?? 0;

            // Reimbursements
            reimbursementsByEmployee.TryGetValue(employee.Id, out var empReimbursements);
            decimal reimbursementTotal = empReimbursements?.Sum(r => r.Amount) ?? 0m;

            // Earnings
            decimal grossSalary = baseSalary + hra + allowances;

            // Deductions (Statutory Indian deductions: PF = 12% of basic, TDS = ~5-10% depending on slab)
            decimal pf = Math.Round(baseSalary * 0.12m, 2);
            decimal esi = grossSalary <= 21000m ? Math.Round(grossSalary * 0.0075m, 2) : 0m;
            decimal tds = Math.Round(grossSalary * 0.05m, 2); // 5% TDS estimation

            if (existingPayslips.TryGetValue(employee.Id, out var existing))
            {
                _context.Payslips.Remove(existing);
            }

            var payslip = Payslip.CreateDetailed(
                employee.Id,
                request.Month,
                request.Year,
                basicSalary: baseSalary,
                hra: hra,
                allowances: allowances,
                bonusPay: 0m,
                overtimePay: 0m,
                pfDeduction: pf,
                esiDeduction: esi,
                tdsDeduction: tds,
                reimbursementsAmount: reimbursementTotal,
                workingDays: daysInMonth,
                presentDays: presentDays,
                paidLeaveDays: paidLeaveDays,
                unpaidLeaveDays: unpaidLeaveDays,
                status: "Calculated",
                remarks: $"Processed on {DateTime.UtcNow:dd-MMM-yyyy}"
            );

            _context.Payslips.Add(payslip);
            generatedPayslips.Add(payslip);

            // Mark associated reimbursements as attached to this payroll period
            if (empReimbursements != null)
            {
                foreach (var r in empReimbursements)
                {
                    r.MarkPaid(periodString);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Build return summary
        var departments = await _context.Departments.ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);
        var deptBreakdown = employees
            .GroupBy(e => e.DepartmentId)
            .Select(g =>
            {
                var deptName = departments.TryGetValue(g.Key, out var name) ? name : "General";
                var empIds = g.Select(e => e.Id).ToHashSet();
                var deptPayslips = generatedPayslips.Where(p => empIds.Contains(p.EmployeeId)).ToList();
                return new DepartmentPayrollDto
                {
                    DepartmentName = deptName,
                    EmployeeCount = g.Count(),
                    TotalGross = deptPayslips.Sum(p => p.GrossSalary),
                    TotalNet = deptPayslips.Sum(p => p.NetSalary)
                };
            }).ToList();

        var payslipDtos = generatedPayslips.Select(p =>
        {
            var emp = employees.FirstOrDefault(e => e.Id == p.EmployeeId);
            return new PayslipDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeName = emp?.FullName ?? "Employee",
                EmployeeCode = emp?.EmployeeCode ?? "",
                DepartmentName = emp != null && departments.TryGetValue(emp.DepartmentId, out var dname) ? dname : "General",
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

        return new PayrollSummaryDto
        {
            Month = request.Month,
            Year = request.Year,
            TotalEmployees = employees.Count,
            ProcessedCount = generatedPayslips.Count,
            TotalGrossSalary = generatedPayslips.Sum(p => p.GrossSalary),
            TotalDeductions = generatedPayslips.Sum(p => p.Deductions),
            TotalPf = generatedPayslips.Sum(p => p.PfDeduction),
            TotalEsi = generatedPayslips.Sum(p => p.EsiDeduction),
            TotalTds = generatedPayslips.Sum(p => p.TdsDeduction),
            TotalReimbursements = generatedPayslips.Sum(p => p.ReimbursementsAmount),
            TotalNetSalary = generatedPayslips.Sum(p => p.NetSalary),
            Status = "Calculated",
            DepartmentBreakdown = deptBreakdown,
            Payslips = payslipDtos
        };
    }
}
