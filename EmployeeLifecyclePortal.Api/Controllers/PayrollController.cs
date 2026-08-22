using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Queries.Payroll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class PayrollController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public PayrollController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet("summary")]
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

    [HttpGet("salary-structure/{employeeId:guid}")]
    public async Task<IActionResult> GetSalaryStructure(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
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

    [HttpGet("payslips")]
    public async Task<IActionResult> GetPayslips(
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
    {
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
        var result = await _mediator.Send(
            new GetPayslipsByMonthQuery(month, year),
            cancellationToken);

        return Ok(result);
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

    [HttpGet("reimbursements")]
    public async Task<IActionResult> GetReimbursements(
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
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
        var result = await _mediator.Send(
            new CreateReimbursementCommand(
                request.EmployeeId,
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
