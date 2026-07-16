using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 26: Training - Courses, certifications, assessments</summary>
public class TrainingCourse : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public int DurationDays { get; private set; }
    public string Status { get; private set; } = "Active";
    public DateTime StartDateUtc { get; private set; }
    public DateTime EndDateUtc { get; private set; }

    private TrainingCourse() { }

    public TrainingCourse(string title, string description, string provider, int duration, DateTime startDate, DateTime endDate)
    {
        Title = title;
        Description = description;
        Provider = provider;
        DurationDays = duration;
        StartDateUtc = startDate;
        EndDateUtc = endDate;
    }
}

public class EmployeeTraining : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid TrainingCourseId { get; private set; }
    public string Status { get; private set; } = "Enrolled";
    public DateTime EnrolledDateUtc { get; private set; }
    public DateTime? CompletedDateUtc { get; private set; }
    public int? Score { get; private set; }
    public string? CertificatePath { get; private set; }
    public Employee? Employee { get; private set; }
    public TrainingCourse? TrainingCourse { get; private set; }

    private EmployeeTraining() { }

    public EmployeeTraining(Guid employeeId, Guid trainingCourseId)
    {
        EmployeeId = employeeId;
        TrainingCourseId = trainingCourseId;
        EnrolledDateUtc = DateTime.UtcNow;
    }

    public void Complete(int score, string certificatePath)
    {
        Status = "Completed";
        CompletedDateUtc = DateTime.UtcNow;
        Score = score;
        CertificatePath = certificatePath;
    }
}

public class Certification : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public string CertificationName { get; private set; } = string.Empty;
    public string IssuingAuthority { get; private set; } = string.Empty;
    public DateTime IssuedDateUtc { get; private set; }
    public DateTime? ExpiryDateUtc { get; private set; }
    public string CertificateNumber { get; private set; } = string.Empty;
    public Employee? Employee { get; private set; }

    private Certification() { }

    public Certification(Guid employeeId, string name, string authority, DateTime issuedDate, DateTime? expiryDate)
    {
        EmployeeId = employeeId;
        CertificationName = name;
        IssuingAuthority = authority;
        IssuedDateUtc = issuedDate;
        ExpiryDateUtc = expiryDate;
    }

    public bool IsValid()
    {
        if (!ExpiryDateUtc.HasValue) return true;
        return ExpiryDateUtc > DateTime.UtcNow;
    }
}
