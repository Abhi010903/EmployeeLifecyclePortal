namespace EmployeeLifecyclePortal.Application.DTOs.Reports;

public class ReportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Generated";
    public DateTime GeneratedDateUtc { get; set; }
    public string? DataJson { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class ReportDataDto
{
    public Dictionary<string, object?> Data { get; set; } = new();
    public List<ReportChartDataDto> ChartData { get; set; } = new();
    public ReportSummaryDto? Summary { get; set; }
}

public class ReportChartDataDto
{
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "bar"; // bar, line, pie, doughnut
    public List<string> Labels { get; set; } = new();
    public List<double> Values { get; set; } = new();
    public string? Color { get; set; }
}

public class ReportSummaryDto
{
    public int TotalRecords { get; set; }
    public string? KeyMetric { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? AverageAmount { get; set; }
}
