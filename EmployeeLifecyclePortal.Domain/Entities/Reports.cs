using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 29 & 30: Reporting, Analytics, Dashboard</summary>
public class Report : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Generated";
    public DateTime GeneratedDateUtc { get; private set; }
    public string? DataJson { get; private set; }
    public Guid GeneratedByUserId { get; private set; }

    private Report() { }

    public static Report Create(string name, string type, string description, Guid generatedByUserId)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Description = description,
            GeneratedDateUtc = DateTime.UtcNow,
            GeneratedByUserId = generatedByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void SetData(string dataJson)
    {
        DataJson = dataJson;
    }
}

public class DashboardWidget : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Configuration { get; private set; }

    private DashboardWidget() { }

    public DashboardWidget(string title, string type, int order)
    {
        Title = title;
        Type = type;
        Order = order;
    }
}

public class DepartmentAnalytics : AuditableEntity
{
    public Guid DepartmentId { get; private set; }
    public int TotalEmployees { get; private set; }
    public int ActiveEmployees { get; private set; }
    public int InactiveEmployees { get; private set; }
    public decimal AverageSalary { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public Department? Department { get; private set; }

    private DepartmentAnalytics() { }

    public DepartmentAnalytics(Guid departmentId, int month, int year)
    {
        DepartmentId = departmentId;
        Month = month;
        Year = year;
    }

    public void UpdateMetrics(int total, int active, int inactive, decimal avgSalary)
    {
        TotalEmployees = total;
        ActiveEmployees = active;
        InactiveEmployees = inactive;
        AverageSalary = avgSalary;
    }
}
