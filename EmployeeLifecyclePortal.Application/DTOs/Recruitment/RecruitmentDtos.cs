namespace EmployeeLifecyclePortal.Application.DTOs.Recruitment;

public sealed class JobPostingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime PostedDateUtc { get; set; }
    public DateTime? ClosedDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}

public sealed class CandidateDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Status { get; set; } = "Applied";
    public Guid JobPostingId { get; set; }
    public string? JobPostingTitle { get; set; }
    public string? ResumePath { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}

public sealed class InterviewDto
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string? CandidateName { get; set; }
    public DateTime ScheduledDateUtc { get; set; }
    public string InterviewerName { get; set; } = string.Empty;
    public string Status { get; set; } = "Scheduled";
    public int? Rating { get; set; }
    public string? Feedback { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}

public sealed class JobOfferDto
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string? CandidateName { get; set; }
    public decimal OfferedSalary { get; set; }
    public DateTime OfferDateUtc { get; set; }
    public DateTime? ExpiryDateUtc { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
