namespace EmployeeLifecyclePortal.Application.DTOs.Attendance;

public sealed class AttendanceDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public DateTime CheckInTimeUtc { get; set; }

    public DateTime? CheckOutTimeUtc { get; set; }

    public string Status { get; set; } = "Present";

    public bool IsApproved { get; set; }

    public string? Notes { get; set; }

    public decimal HoursWorked { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
