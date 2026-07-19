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

        // Placeholder for Excel export - would need EPPlus or similar library
        var bytes = System.Text.Encoding.UTF8.GetBytes(request.Data);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpPost("export/pdf")]
    public IActionResult ExportPdf([FromBody] ExportRequest request)
    {
        if (string.IsNullOrEmpty(request.Data))
            return BadRequest("No data to export");

        // Placeholder for PDF export - would need iTextSharp or similar library
        var bytes = System.Text.Encoding.UTF8.GetBytes(request.Data);
        return File(bytes, "application/pdf", 
            $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    private static string ConvertToCsv(string jsonData)
    {
        // Simple CSV conversion from JSON
        var lines = new List<string> { "Key,Value" };
        var pairs = jsonData.Split(',');
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
    public string Format { get; set; } = "csv"; // csv, excel, pdf
}
