namespace EmployeeLifecyclePortal.Application.DTOs.Dashboard;

/// <summary>
/// Recent activity event data for the activity timeline/log.
/// Tracks important HR actions in the system.
/// </summary>
public sealed class RecentActivityDto
{
    /// <summary>
    /// Unique identifier for the activity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of activity (e.g., "Employee Created", "Leave Applied", "Department Created").
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;

    /// <summary>
    /// Description of the activity.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// User who performed the action.
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the activity occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Icon/category for UI display (e.g., "user-added", "document-created", "leave-request").
    /// </summary>
    public string Category { get; set; } = string.Empty;
}
