using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 25: Performance management - Goals, reviews, KPIs</summary>
public class PerformanceGoal : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDateUtc { get; private set; }
    public DateTime EndDateUtc { get; private set; }
    public string Status { get; private set; } = "Active";
    public int ProgressPercentage { get; private set; }
    public Employee? Employee { get; private set; }

    private PerformanceGoal() { }

    public PerformanceGoal(Guid employeeId, string title, string description, DateTime startDate, DateTime endDate)
    {
        EmployeeId = employeeId;
        Title = title;
        Description = description;
        StartDateUtc = startDate;
        EndDateUtc = endDate;
    }

    public void UpdateProgress(int percentage)
    {
        ProgressPercentage = Math.Min(100, percentage);
    }

    public void Complete()
    {
        Status = "Completed";
        ProgressPercentage = 100;
    }
}

public class PerformanceReview : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public int Year { get; private set; }
    public int Quarter { get; private set; }
    public int Rating { get; private set; }
    public string Comments { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Draft";
    public Employee? Employee { get; private set; }

    private PerformanceReview() { }

    public PerformanceReview(Guid employeeId, int year, int quarter, int rating)
    {
        EmployeeId = employeeId;
        Year = year;
        Quarter = quarter;
        Rating = Math.Min(5, rating);
    }

    public void Submit()
    {
        Status = "Submitted";
    }

    public void Approve(Guid reviewedByUserId)
    {
        Status = "Approved";
        ReviewedByUserId = reviewedByUserId;
    }
}

public class KPI : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Target { get; private set; }
    public decimal Achieved { get; private set; }
    public int Year { get; private set; }
    public Employee? Employee { get; private set; }

    private KPI() { }

    public KPI(Guid employeeId, string name, decimal target, int year)
    {
        EmployeeId = employeeId;
        Name = name;
        Target = target;
        Year = year;
    }

    public decimal GetAchievementPercentage()
    {
        if (Target == 0) return 0;
        return (Achieved / Target) * 100;
    }

    public void UpdateAchievement(decimal achieved)
    {
        Achieved = achieved;
    }
}
