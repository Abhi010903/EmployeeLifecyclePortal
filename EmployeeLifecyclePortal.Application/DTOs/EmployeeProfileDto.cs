namespace EmployeeLifecyclePortal.Application.DTOs;

/// <summary>
/// DTO for displaying complete employee profile information
/// </summary>
public sealed class EmployeeProfileDto
{
    public Guid Id { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public Guid? ManagerId { get; set; }

    public string? ManagerName { get; set; }

    public Guid? TeamLeadId { get; set; }

    public string? TeamLeadName { get; set; }

    public List<EmployeeRoleDto> Roles { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    public int TimelineEventsCount { get; set; }

    public int DocumentsCount { get; set; }

    public int SubordinatesCount { get; set; }
}

public sealed class EmployeeRoleDto
{
    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string RoleDescription { get; set; } = string.Empty;
}

/// <summary>
/// DTO for employee timeline events
/// </summary>
public sealed class EmployeeTimelineDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime EventDateUtc { get; set; }

    public string? Category { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string TimeAgo
    {
        get
        {
            var elapsed = DateTime.UtcNow - EventDateUtc;

            return elapsed.TotalSeconds < 60
                ? $"{(int)elapsed.TotalSeconds} seconds ago"
                : elapsed.TotalMinutes < 60
                    ? $"{(int)elapsed.TotalMinutes} minutes ago"
                    : elapsed.TotalHours < 24
                        ? $"{(int)elapsed.TotalHours} hours ago"
                        : $"{(int)elapsed.TotalDays} days ago";
        }
    }
}

/// <summary>
/// DTO for employee documents
/// </summary>
public sealed class EmployeeDocumentDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DateTime? ExpirationDateUtc { get; set; }

    public string? Notes { get; set; }

    public bool IsArchived { get; set; }

    public bool IsExpired { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string FormattedFileSize
    {
        get
        {
            const long kb = 1024;
            const long mb = kb * 1024;
            const long gb = mb * 1024;

            return FileSizeBytes switch
            {
                >= gb => $"{(double)FileSizeBytes / gb:F2} GB",
                >= mb => $"{(double)FileSizeBytes / mb:F2} MB",
                >= kb => $"{(double)FileSizeBytes / kb:F2} KB",
                _ => $"{FileSizeBytes} bytes"
            };
        }
    }
}
