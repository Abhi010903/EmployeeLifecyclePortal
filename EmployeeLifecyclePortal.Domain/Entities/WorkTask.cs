using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

public class WorkTask : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid EmployeeId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Guid? ManagerId { get; private set; }
    public string Priority { get; set; } = "Medium";
    public DateTime StartDateUtc { get; private set; }
    public DateTime DeadlineUtc { get; private set; }
    public string Status { get; set; } = "Pending";
    public int CompletionPercentage { get; set; }
    public string? Comments { get; set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public Employee? Employee { get; private set; }
    public Department? Department { get; private set; }
    public Employee? Manager { get; private set; }

    private WorkTask() { }

    public WorkTask(
        string title,
        string description,
        Guid employeeId,
        Guid? departmentId,
        Guid? managerId,
        string priority,
        DateTime startDateUtc,
        DateTime deadlineUtc)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        EmployeeId = employeeId;
        DepartmentId = departmentId;
        ManagerId = managerId;
        Priority = priority;
        StartDateUtc = startDateUtc;
        DeadlineUtc = deadlineUtc;
        Status = "Pending";
        CompletionPercentage = 0;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateProgress(int percentage, string status, string? comments = null)
    {
        CompletionPercentage = Math.Clamp(percentage, 0, 100);
        Status = status;
        if (comments != null)
        {
            Comments = comments;
        }

        if (percentage >= 100 || status == "Completed")
        {
            Status = "Completed";
            CompletionPercentage = 100;
            CompletedAtUtc = DateTime.UtcNow;
        }
    }
}
