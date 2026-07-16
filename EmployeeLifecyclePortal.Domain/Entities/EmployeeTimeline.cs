using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>
/// Tracks major lifecycle events for an employee.
/// Records important moments like joining, promotion, transfer, training completion, performance reviews, etc.
/// </summary>
public class EmployeeTimeline : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// Type of event: "Joined", "Promoted", "Transferred", "Training", "Review", "Leave", "Award", "Milestone"
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// Title or name of the event
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Detailed description of the event
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Date when the event occurred (may be different from CreatedAtUtc)
    /// </summary>
    public DateTime EventDateUtc { get; private set; }

    /// <summary>
    /// Optional associated category (e.g., department name for transfer, skill name for training)
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Navigation property to the employee
    /// </summary>
    public Employee? Employee { get; private set; }

    private EmployeeTimeline()
    {
    }

    /// <summary>
    /// Creates a new timeline event for an employee.
    /// </summary>
    public static EmployeeTimeline CreateEvent(
        Guid employeeId,
        string eventType,
        string title,
        string description,
        DateTime eventDateUtc,
        string? category = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID is required.", nameof(employeeId));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        var validEventTypes = new[] { "Joined", "Promoted", "Transferred", "Training", "Review", "Leave", "Award", "Milestone", "Other" };

        if (!validEventTypes.Contains(eventType))
            throw new ArgumentException(
                $"Event type must be one of: {string.Join(", ", validEventTypes)}",
                nameof(eventType));

        return new EmployeeTimeline
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            EventType = eventType,
            Title = title,
            Description = description,
            EventDateUtc = eventDateUtc,
            Category = category,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(
        string title,
        string description,
        DateTime eventDateUtc,
        string? category = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        Title = title;
        Description = description;
        EventDateUtc = eventDateUtc;
        Category = category;
    }
}
