namespace EmployeeLifecyclePortal.Application.DTOs.Attendance;

public sealed class LeaveTypeDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DaysPerYear { get; set; }

    public bool IsPaid { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
