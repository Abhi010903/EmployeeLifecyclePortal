namespace EmployeeLifecyclePortal.Application.DTOs.Tasks;

public sealed class WorkTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime StartDateUtc { get; set; }
    public DateTime DeadlineUtc { get; set; }
    public string Status { get; set; } = "Pending";
    public int CompletionPercentage { get; set; }
    public string? Comments { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
