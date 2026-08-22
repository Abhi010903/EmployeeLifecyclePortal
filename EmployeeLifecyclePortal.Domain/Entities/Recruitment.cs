using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 24: Recruitment - Job postings, candidates, interviews, offers</summary>
public class JobPosting : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid DepartmentId { get; private set; }
    public string Status { get; private set; } = "Open";
    public DateTime PostedDateUtc { get; private set; }
    public DateTime? ClosedDateUtc { get; private set; }
    public Department? Department { get; private set; }

    private JobPosting() { }

    public JobPosting(string title, string description, Guid departmentId)
    {
        Title = title;
        Description = description;
        DepartmentId = departmentId;
        PostedDateUtc = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = "Closed";
        ClosedDateUtc = DateTime.UtcNow;
    }
}

public class Candidate : AuditableEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string Status { get; private set; } = "Applied";
    public Guid JobPostingId { get; private set; }
    public string? ResumePath { get; private set; }
    public JobPosting? JobPosting { get; private set; }

    private Candidate() { }

    public Candidate(string firstName, string lastName, string email, Guid jobPostingId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        JobPostingId = jobPostingId;
    }

    public void MoveToInterview()
    {
        Status = "Interview";
    }

    public void Reject()
    {
        Status = "Rejected";
    }

    public void UpdateStatus(string status)
    {
        Status = status;
    }

    public void SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }
}

public class Interview : AuditableEntity
{
    public Guid CandidateId { get; private set; }
    public DateTime ScheduledDateUtc { get; private set; }
    public string InterviewerName { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Scheduled";
    public int? Rating { get; private set; }
    public string? Feedback { get; private set; }
    public Candidate? Candidate { get; private set; }

    private Interview() { }

    public Interview(Guid candidateId, DateTime scheduledDate, string interviewerName)
    {
        CandidateId = candidateId;
        ScheduledDateUtc = scheduledDate;
        InterviewerName = interviewerName;
    }

    public void Complete(int rating, string feedback)
    {
        Status = "Completed";
        Rating = rating;
        Feedback = feedback;
    }
}

public class JobOffer : AuditableEntity
{
    public Guid CandidateId { get; private set; }
    public decimal OfferedSalary { get; private set; }
    public DateTime OfferDateUtc { get; private set; }
    public DateTime? ExpiryDateUtc { get; private set; }
    public string Status { get; private set; } = "Pending";
    public Candidate? Candidate { get; private set; }

    private JobOffer() { }

    public JobOffer(Guid candidateId, decimal salary, DateTime expiryDate)
    {
        CandidateId = candidateId;
        OfferedSalary = salary;
        OfferDateUtc = DateTime.UtcNow;
        ExpiryDateUtc = expiryDate;
    }

    public void Accept()
    {
        Status = "Accepted";
    }

    public void Reject()
    {
        Status = "Rejected";
    }
}
