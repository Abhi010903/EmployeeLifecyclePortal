using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 21: Attendance tracking with check-in/out and shift management</summary>
public class Attendance : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public DateTime CheckInTimeUtc { get; private set; }
    public DateTime? CheckOutTimeUtc { get; private set; }
    public string Status { get; private set; } = "Present";
    public bool IsApproved { get; private set; }
    public string? Notes { get; private set; }
    public Employee? Employee { get; private set; }

    private Attendance() { }

    public static Attendance CreateCheckIn(Guid employeeId)
    {
        return new Attendance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            CheckInTimeUtc = DateTime.UtcNow,
            Status = "Present",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void CheckOut()
    {
        CheckOutTimeUtc = DateTime.UtcNow;
    }

    public void Approve()
    {
        IsApproved = true;
    }

    public decimal GetHoursWorked()
    {
        if (!CheckOutTimeUtc.HasValue) return 0;
        return (decimal)(CheckOutTimeUtc.Value - CheckInTimeUtc).TotalHours;
    }
}

public class LeaveType : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public int DaysPerYear { get; private set; }
    public bool IsPaid { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private LeaveType() { }

    public LeaveType(string name, int daysPerYear, bool isPaid, string description)
    {
        Name = name;
        DaysPerYear = daysPerYear;
        IsPaid = isPaid;
        Description = description;
    }
}

public class LeaveRequest : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public DateTime StartDateUtc { get; private set; }
    public DateTime EndDateUtc { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string? Reason { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public Employee? Employee { get; private set; }
    public LeaveType? LeaveType { get; private set; }

    private LeaveRequest() { }

    public static LeaveRequest CreateRequest(Guid employeeId, Guid leaveTypeId, DateTime startDate, DateTime endDate, string reason)
    {
        return new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            StartDateUtc = startDate,
            EndDateUtc = endDate,
            Status = "Pending",
            Reason = reason,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Approve(Guid approvedByUserId)
    {
        Status = "Approved";
        ApprovedByUserId = approvedByUserId;
    }

    public void Reject()
    {
        Status = "Rejected";
    }

    public int GetDaysRequested()
    {
        return (int)(EndDateUtc - StartDateUtc).TotalDays + 1;
    }
}

public class LeaveBalance : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public int TotalDays { get; private set; }
    public int UsedDays { get; private set; }
    public int RemainingDays => TotalDays - UsedDays;
    public int Year { get; private set; }
    public Employee? Employee { get; private set; }
    public LeaveType? LeaveType { get; private set; }

    private LeaveBalance() { }

    public void UseLeave(int days)
    {
        UsedDays += days;
    }

    public bool HasSufficientBalance(int daysRequested)
    {
        return RemainingDays >= daysRequested;
    }
}
