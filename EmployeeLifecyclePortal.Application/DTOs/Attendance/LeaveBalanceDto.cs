namespace EmployeeLifecyclePortal.Application.DTOs.Attendance;

public sealed class LeaveBalanceDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public Guid LeaveTypeId { get; set; }

    public string? LeaveTypeName { get; set; }

    public int TotalDays { get; set; }

    public int UsedDays { get; set; }

    public int RemainingDays { get; set; }

    public int Year { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
