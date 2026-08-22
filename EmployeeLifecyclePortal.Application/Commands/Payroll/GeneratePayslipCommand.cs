using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Payroll;

public sealed record GeneratePayslipCommand(
    Guid EmployeeId,
    int Month,
    int Year) : IRequest<PayslipDto>;

public sealed class GeneratePayslipCommandHandler : IRequestHandler<GeneratePayslipCommand, PayslipDto>
{
    private readonly IApplicationDbContext _context;

    public GeneratePayslipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayslipDto> Handle(GeneratePayslipCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null)
            throw new InvalidOperationException("Employee not found.");

        // Check if payslip for this month/year already exists
        var existingPayslip = await _context.Payslips
            .FirstOrDefaultAsync(p => p.EmployeeId == request.EmployeeId && p.Month == request.Month && p.Year == request.Year, cancellationToken);

        if (existingPayslip != null)
        {
            return new PayslipDto
            {
                Id = existingPayslip.Id,
                EmployeeId = existingPayslip.EmployeeId,
                EmployeeName = employee.FullName,
                Month = existingPayslip.Month,
                Year = existingPayslip.Year,
                GrossSalary = existingPayslip.GrossSalary,
                Deductions = existingPayslip.Deductions,
                NetSalary = existingPayslip.NetSalary,
                Status = existingPayslip.Status,
                GeneratedDateUtc = existingPayslip.GeneratedDateUtc,
                CreatedAtUtc = existingPayslip.CreatedAtUtc,
                CreatedBy = existingPayslip.CreatedBy,
                LastModifiedAtUtc = existingPayslip.LastModifiedAtUtc,
                LastModifiedBy = existingPayslip.LastModifiedBy
            };
        }

        // Get salary structure
        var salaryStructure = await _context.SalaryStructures
            .Where(s => s.EmployeeId == request.EmployeeId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        decimal grossSalary = salaryStructure?.BaseSalary ?? 50000m;
        // Standard deductions (e.g. 10% tax/PF)
        decimal deductions = Math.Round(grossSalary * 0.10m, 2);

        var payslip = Payslip.Create(request.EmployeeId, request.Month, request.Year, grossSalary, deductions);
        _context.Payslips.Add(payslip);
        await _context.SaveChangesAsync(cancellationToken);

        return new PayslipDto
        {
            Id = payslip.Id,
            EmployeeId = payslip.EmployeeId,
            EmployeeName = employee.FullName,
            Month = payslip.Month,
            Year = payslip.Year,
            GrossSalary = payslip.GrossSalary,
            Deductions = payslip.Deductions,
            NetSalary = payslip.NetSalary,
            Status = payslip.Status,
            GeneratedDateUtc = payslip.GeneratedDateUtc,
            CreatedAtUtc = payslip.CreatedAtUtc,
            CreatedBy = payslip.CreatedBy
        };
    }
}
