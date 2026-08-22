using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 23: Payroll management - Salary, components, processing</summary>
public class SalaryStructure : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public decimal BaseSalary { get; private set; }
    public string Currency { get; private set; } = "INR";
    public DateTime EffectiveFromUtc { get; private set; }
    public DateTime? EffectiveToUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Employee? Employee { get; private set; }

    private SalaryStructure() { }

    public SalaryStructure(Guid employeeId, decimal baseSalary, DateTime effectiveFrom)
    {
        EmployeeId = employeeId;
        BaseSalary = baseSalary;
        EffectiveFromUtc = effectiveFrom;
    }

    public void End()
    {
        IsActive = false;
        EffectiveToUtc = DateTime.UtcNow;
    }
}

public class SalaryComponent : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = "Earning";
    public decimal Amount { get; private set; }
    public bool IsVariable { get; private set; }
    public Guid SalaryStructureId { get; private set; }
    public SalaryStructure? SalaryStructure { get; private set; }

    private SalaryComponent() { }

    public SalaryComponent(string name, string type, decimal amount, bool isVariable, Guid salaryStructureId)
    {
        Name = name;
        Type = type;
        Amount = amount;
        IsVariable = isVariable;
        SalaryStructureId = salaryStructureId;
    }
}

public class Payslip : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal Deductions { get; private set; }
    public decimal NetSalary { get; private set; }
    public string Status { get; private set; } = "Generated";
    public DateTime GeneratedDateUtc { get; private set; }
    public Employee? Employee { get; private set; }

    private Payslip() { }

    public static Payslip Create(Guid employeeId, int month, int year, decimal gross, decimal deductions)
    {
        return new Payslip
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Month = month,
            Year = year,
            GrossSalary = gross,
            Deductions = deductions,
            NetSalary = gross - deductions,
            GeneratedDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
