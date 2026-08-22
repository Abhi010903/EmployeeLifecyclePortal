using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

public class StaffingRequest : AuditableEntity
{
    public Guid DepartmentId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public int CurrentHeadcount { get; private set; }
    public int RequiredCount { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";
    public string? AdminComments { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }

    public Department? Department { get; private set; }

    private StaffingRequest() { }

    public StaffingRequest(
        Guid departmentId,
        Guid requestedByUserId,
        int currentHeadcount,
        int requiredCount,
        string reason)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        RequestedByUserId = requestedByUserId;
        CurrentHeadcount = currentHeadcount;
        RequiredCount = requiredCount;
        Reason = reason;
        Status = "Pending";
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Approve(string? comments = null)
    {
        Status = "Approved";
        AdminComments = comments;
        ResolvedAtUtc = DateTime.UtcNow;
    }

    public void Reject(string? comments = null)
    {
        Status = "Rejected";
        AdminComments = comments;
        ResolvedAtUtc = DateTime.UtcNow;
    }
}
