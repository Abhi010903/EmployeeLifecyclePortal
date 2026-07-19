namespace EmployeeLifecyclePortal.Application.DTOs.Performance;

public sealed class PerformanceReviewDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public string? ReviewedByName { get; set; }

    public int Year { get; set; }

    public int Quarter { get; set; }

    public int Rating { get; set; }

    public string Comments { get; set; } = string.Empty;

    public string Status { get; set; } = "Draft";

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
