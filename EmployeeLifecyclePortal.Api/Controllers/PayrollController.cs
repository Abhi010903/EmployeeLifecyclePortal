using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Payroll;
using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Payroll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class PayrollController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public PayrollController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _context = context;
    }

    [HttpGet("summary")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] int? month,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var result = await _mediator.Send(
            new GetPayrollSummaryQuery(targetMonth, targetYear),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyPayrollSummary(
        [FromQuery] int? month,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == empId, cancellationToken);

        var department = employee != null
            ? await _context.Departments.FirstOrDefaultAsync(d => d.Id == employee.DepartmentId, cancellationToken)
            : null;

        var salaryStructure = await _mediator.Send(
            new GetSalaryStructureQuery(empId),
            cancellationToken);

        var payslips = await _mediator.Send(
            new GetPayslipsQuery(empId),
            cancellationToken);

        var reimbursements = await _mediator.Send(
            new GetReimbursementsQuery(empId),
            cancellationToken);

        var latestPayslip = payslips.FirstOrDefault(p => p.Month == targetMonth && p.Year == targetYear)
            ?? payslips.FirstOrDefault();

        var baseSalary = salaryStructure?.BaseSalary ?? 50000m;
        var hra = salaryStructure?.Hra ?? Math.Round(baseSalary * 0.40m, 2);
        var specialAllowance = salaryStructure?.SpecialAllowance ?? Math.Round(baseSalary * 0.15m, 2);
        var conveyance = salaryStructure?.ConveyanceAllowance ?? 1600m;
        var gross = latestPayslip?.GrossSalary ?? (baseSalary + hra + specialAllowance + conveyance);
        var pf = latestPayslip?.PfDeduction ?? Math.Round(baseSalary * 0.12m, 2);
        var esi = latestPayslip?.EsiDeduction ?? (gross <= 21000 ? Math.Round(gross * 0.0075m, 2) : 0m);
        var tds = latestPayslip?.TdsDeduction ?? Math.Round(gross * 0.05m, 2);
        var totalDeductions = latestPayslip?.Deductions ?? (pf + esi + tds);
        var net = latestPayslip?.NetSalary ?? (gross - totalDeductions);
        var totalReimbursements = reimbursements.Where(r => r.Status == "Approved" || r.Status == "Paid").Sum(r => r.Amount);

        var summary = new EmployeePayrollSummaryDto
        {
            EmployeeId = empId,
            EmployeeName = employee?.FullName ?? "Employee",
            EmployeeCode = employee?.EmployeeCode ?? "",
            DepartmentName = department?.Name ?? "General",
            Month = targetMonth,
            Year = targetYear,
            GrossSalary = gross,
            TotalDeductions = totalDeductions,
            NetSalary = net,
            TotalReimbursements = totalReimbursements,
            BasicSalary = baseSalary,
            Hra = hra,
            SpecialAllowance = specialAllowance,
            ConveyanceAllowance = conveyance,
            OvertimePay = latestPayslip?.OvertimePay ?? 0m,
            BonusPay = latestPayslip?.BonusPay ?? 0m,
            PfDeduction = pf,
            EsiDeduction = esi,
            TdsDeduction = tds,
            OtherDeductions = Math.Max(0, totalDeductions - pf - esi - tds),
            LatestPayslip = latestPayslip,
            RecentPayslips = payslips.Take(6).ToList(),
            Reimbursements = reimbursements.Take(10).ToList()
        };

        return Ok(summary);
    }

    [HttpPost("run")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> ProcessPayrollRun(
        [FromBody] ProcessPayrollRunRequest request,
        CancellationToken cancellationToken)
    {
        var targetMonth = request.Month > 0 ? request.Month : DateTime.UtcNow.Month;
        var targetYear = request.Year > 0 ? request.Year : DateTime.UtcNow.Year;

        var result = await _mediator.Send(
            new ProcessPayrollRunCommand(targetMonth, targetYear),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("approve-run")]
    [Authorize(Policy = Permissions.Admin)]
    public async Task<IActionResult> ApprovePayrollRun(
        [FromBody] ProcessPayrollRunRequest request,
        CancellationToken cancellationToken)
    {
        var targetMonth = request.Month > 0 ? request.Month : DateTime.UtcNow.Month;
        var targetYear = request.Year > 0 ? request.Year : DateTime.UtcNow.Year;

        var result = await _mediator.Send(
            new ApprovePayrollRunCommand(targetMonth, targetYear),
            cancellationToken);

        return Ok(new { success = result, message = "Payroll approved successfully." });
    }

    [HttpGet("salary-structure/my")]
    public async Task<IActionResult> GetMySalaryStructure(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var result = await _mediator.Send(
            new GetSalaryStructureQuery(empId),
            cancellationToken);

        if (result == null)
            return NotFound("Salary structure not found.");

        return Ok(result);
    }

    [HttpGet("salary-structure/{employeeId:guid}")]
    public async Task<IActionResult> GetSalaryStructure(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!await _currentUserService.HasAccessToEmployeeAsync(employeeId, _context, cancellationToken))
        {
            return Forbid();
        }

        var result = await _mediator.Send(
            new GetSalaryStructureQuery(employeeId),
            cancellationToken);

        if (result == null)
            return NotFound("Salary structure not found.");

        return Ok(result);
    }

    [HttpPut("salary-structure/{employeeId:guid}")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> UpdateSalaryStructure(
        Guid employeeId,
        [FromBody] UpdateSalaryStructureRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateSalaryStructureCommand(employeeId, request.BaseSalary),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("payslips/my")]
    public async Task<IActionResult> GetMyPayslips(
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var result = await _mediator.Send(
            new GetPayslipsQuery(empId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("payslips")]
    public async Task<IActionResult> GetPayslips(
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        if (!isElevated)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            employeeId = empId;
        }

        var result = await _mediator.Send(
            new GetPayslipsQuery(employeeId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("payslips/{year:int}/{month:int}")]
    public async Task<IActionResult> GetPayslipsByMonth(
        int month,
        int year,
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        if (!isElevated)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            var myPayslips = await _mediator.Send(
                new GetPayslipsQuery(empId),
                cancellationToken);

            var specificPayslip = myPayslips.Where(p => p.Month == month && p.Year == year).ToList();
            return Ok(specificPayslip);
        }

        var result = await _mediator.Send(
            new GetPayslipsByMonthQuery(month, year),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("payslips/{id:guid}")]
    public async Task<IActionResult> GetPayslipById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var payslip = await _context.Payslips
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (payslip == null)
            return NotFound("Payslip not found.");

        if (!await _currentUserService.HasAccessToEmployeeAsync(payslip.EmployeeId, _context, cancellationToken))
        {
            return Forbid();
        }

        var emp = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == payslip.EmployeeId, cancellationToken);

        var deptName = "General";
        if (emp != null)
        {
            var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == emp.DepartmentId, cancellationToken);
            deptName = dept?.Name ?? "General";
        }

        var dto = new PayslipDto
        {
            Id = payslip.Id,
            EmployeeId = payslip.EmployeeId,
            EmployeeName = emp?.FullName ?? "Unknown",
            EmployeeCode = emp?.EmployeeCode ?? "",
            DepartmentName = deptName,
            Month = payslip.Month,
            Year = payslip.Year,
            BasicSalary = payslip.BasicSalary,
            Hra = payslip.Hra,
            Allowances = payslip.Allowances,
            BonusPay = payslip.BonusPay,
            OvertimePay = payslip.OvertimePay,
            GrossSalary = payslip.GrossSalary,
            PfDeduction = payslip.PfDeduction,
            EsiDeduction = payslip.EsiDeduction,
            TdsDeduction = payslip.TdsDeduction,
            Deductions = payslip.Deductions,
            ReimbursementsAmount = payslip.ReimbursementsAmount,
            NetSalary = payslip.NetSalary,
            WorkingDays = payslip.WorkingDays,
            PresentDays = payslip.PresentDays,
            PaidLeaveDays = payslip.PaidLeaveDays,
            UnpaidLeaveDays = payslip.UnpaidLeaveDays,
            Status = payslip.Status,
            PaymentMethod = payslip.PaymentMethod,
            PaymentDateUtc = payslip.PaymentDateUtc,
            Remarks = payslip.Remarks,
            GeneratedDateUtc = payslip.GeneratedDateUtc,
            CreatedAtUtc = payslip.CreatedAtUtc,
            CreatedBy = payslip.CreatedBy
        };

        return Ok(dto);
    }

    [HttpPost("generate-payslip")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> GeneratePayslip(
        [FromBody] GeneratePayslipRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GeneratePayslipCommand(request.EmployeeId, request.Month, request.Year),
            cancellationToken);

        return Ok(result);
    }

    // --- REIMBURSEMENTS WORKFLOW ---

    [HttpGet("reimbursements/my")]
    public async Task<IActionResult> GetMyReimbursements(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        var result = await _mediator.Send(
            new GetReimbursementsQuery(empId, status),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("reimbursements")]
    public async Task<IActionResult> GetReimbursements(
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager" ||
                         _currentUserService.Role == "Team Lead" ||
                         _currentUserService.Role == "TeamLead";

        if (!isElevated)
        {
            var empId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
            employeeId = empId;
        }

        var result = await _mediator.Send(
            new GetReimbursementsQuery(employeeId, status),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("reimbursements")]
    public async Task<IActionResult> CreateReimbursement(
        [FromBody] CreateReimbursementRequest request,
        CancellationToken cancellationToken)
    {
        var isElevated = _currentUserService.Role == "Admin" ||
                         _currentUserService.Role == "HR" ||
                         _currentUserService.Role == "Manager";

        var targetEmpId = request.EmployeeId;
        if (!isElevated || targetEmpId == Guid.Empty)
        {
            targetEmpId = await _currentUserService.GetRequiredEmployeeIdAsync(_context, cancellationToken);
        }

        var result = await _mediator.Send(
            new CreateReimbursementCommand(
                targetEmpId,
                request.Amount,
                request.Category,
                request.Description,
                request.ReceiptUrl),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("reimbursements/{id:guid}/approve")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> ApproveReimbursement(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var result = await _mediator.Send(
            new ApproveReimbursementCommand(id, currentUserId),
            cancellationToken);

        if (!result)
            return NotFound("Reimbursement not found.");

        return Ok(new { success = true, message = "Reimbursement approved successfully." });
    }

    [HttpPut("reimbursements/{id:guid}/reject")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> RejectReimbursement(
        Guid id,
        [FromBody] RejectReimbursementRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var result = await _mediator.Send(
            new RejectReimbursementCommand(id, currentUserId, request.Reason ?? "Rejected by supervisor"),
            cancellationToken);

        if (!result)
            return NotFound("Reimbursement not found.");

        return Ok(new { success = true, message = "Reimbursement rejected." });
    }
}

public sealed class ProcessPayrollRunRequest
{
    public int Month { get; set; }
    public int Year { get; set; }
}

public sealed class UpdateSalaryStructureRequest
{
    public decimal BaseSalary { get; set; }
}

public sealed class GeneratePayslipRequest
{
    public Guid EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public sealed class CreateReimbursementRequest
{
    public Guid EmployeeId { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = "Travel";
    public string Description { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
}

public sealed class RejectReimbursementRequest
{
    public string? Reason { get; set; }
}
