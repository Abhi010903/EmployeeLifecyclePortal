using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Payroll;
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

    public PayrollController(IMediator mediator)
    {
        _mediator = mediator;
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
