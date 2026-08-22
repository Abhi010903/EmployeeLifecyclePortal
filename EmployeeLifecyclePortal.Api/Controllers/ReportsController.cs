using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.DTOs.Reports;
using EmployeeLifecyclePortal.Application.Queries.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeeReport(
        [FromQuery] int? departmentId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new GetEmployeeReportQuery(departmentId, status);
        var report = await _mediator.Send(query, cancellationToken);
        return Ok(report);
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
    {
        var query = new GetAttendanceReportQuery(startDate, endDate, employeeId);
        var report = await _mediator.Send(query, cancellationToken);
        return Ok(report);
    }

    [HttpGet("leave")]
    public async Task<IActionResult> GetLeaveReport(
        [FromQuery] int? year,
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
    {
        var query = new GetLeaveReportQuery(year, employeeId);
        var report = await _mediator.Send(query, cancellationToken);
        return Ok(report);
    }

    [HttpGet("payroll")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> GetPayrollReport(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] Guid? employeeId,
        CancellationToken cancellationToken)
    {
        var query = new GetPayrollReportQuery(month, year, employeeId);
        var report = await _mediator.Send(query, cancellationToken);
        return Ok(report);
    }

    [HttpGet("department")]
    public async Task<IActionResult> GetDepartmentReport(
        [FromQuery] int? month,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var query = new GetDepartmentReportQuery(month, year);
        var report = await _mediator.Send(query, cancellationToken);
        return Ok(report);
    }

    [HttpPost("export/csv")]
    public IActionResult ExportCsv([FromBody] ExportRequest request)
    {
        if (string.IsNullOrEmpty(request.Data))
            return BadRequest("No data to export");

        var csv = ConvertToCsv(request.Data);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpPost("export/excel")]
    public IActionResult ExportExcel([FromBody] ExportRequest request)
    {
        if (string.IsNullOrEmpty(request.Data))
            return BadRequest("No data to export");

        var csv = ConvertToCsv(request.Data);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpPost("export/pdf")]
    public IActionResult ExportPdf([FromBody] ExportRequest request)
    {
        if (string.IsNullOrEmpty(request.Data))
            return BadRequest("No data to export");

        var text = "REPORT EXPORT\nGenerated at: " + DateTime.UtcNow.ToString("u") + "\n\n" + ConvertToCsv(request.Data);
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        return File(bytes, "application/pdf", 
            $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    private static string ConvertToCsv(string jsonData)
    {
        var lines = new List<string> { "Key,Value" };
        var pairs = jsonData.Trim('{', '}').Split(',');
        foreach (var pair in pairs)
        {
            lines.Add(pair.Replace("\"", "").Replace(":", ","));
        }
        return string.Join("\n", lines);
    }
}

public sealed class ExportRequest
{
    public string Data { get; set; } = string.Empty;
    public string Format { get; set; } = "csv";
}
