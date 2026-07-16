using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 28: Notifications - Email, SMS, In-App, Templates</summary>
public class NotificationTemplate : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = "Email";
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private NotificationTemplate() { }

    public NotificationTemplate(string name, string type, string subject, string body)
    {
        Name = name;
        Type = type;
        Subject = subject;
        Body = body;
    }
}

public class Notification : AuditableEntity
{
    public Guid RecipientId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Type { get; private set; } = "InApp";
    public string Status { get; private set; } = "Sent";
    public DateTime SentDateUtc { get; private set; }
    public DateTime? ReadDateUtc { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }

    private Notification() { }

    public static Notification Create(Guid recipientId, string title, string message, string type)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = recipientId,
            Title = title,
            Message = message,
            Type = type,
            SentDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        ReadDateUtc = DateTime.UtcNow;
    }

    public bool IsRead()
    {
        return ReadDateUtc.HasValue;
    }
}

public class NotificationLog : AuditableEntity
{
    public Guid NotificationId { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Sent";
    public string? ErrorMessage { get; private set; }
    public DateTime SentAtUtc { get; private set; }

    private NotificationLog() { }

    public NotificationLog(Guid notificationId, string email)
    {
        NotificationId = notificationId;
        RecipientEmail = email;
        SentAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = "Failed";
        ErrorMessage = error;
    }
}
