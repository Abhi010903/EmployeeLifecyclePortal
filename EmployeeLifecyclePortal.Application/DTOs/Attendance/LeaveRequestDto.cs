namespace EmployeeLifecyclePortal.Application.DTOs.Attendance;

public sealed class LeaveRequestDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public Guid LeaveTypeId { get; set; }

    public string? LeaveTypeName { get; set; }

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }

    public int DaysRequested { get; set; }

    public string Status { get; set; } = "Pending";

    public string? Reason { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public string? ApprovedByName { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
