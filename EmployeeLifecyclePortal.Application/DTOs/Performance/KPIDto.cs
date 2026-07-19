namespace EmployeeLifecyclePortal.Application.DTOs.Performance;

public sealed class KPIDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Target { get; set; }

    public decimal Achieved { get; set; }

    public int Year { get; set; }

    public decimal AchievementPercentage { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
