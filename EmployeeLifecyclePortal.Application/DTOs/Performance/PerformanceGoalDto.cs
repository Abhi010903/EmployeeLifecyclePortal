namespace EmployeeLifecyclePortal.Application.DTOs.Performance;

public sealed class PerformanceGoalDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }

    public string Status { get; set; } = "Active";

    public int ProgressPercentage { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
