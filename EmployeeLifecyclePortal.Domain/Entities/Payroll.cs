using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 23: Payroll management - Salary, components, processing</summary>
public class SalaryStructure : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public decimal BaseSalary { get; private set; }
    public decimal Hra { get; private set; }
    public decimal SpecialAllowance { get; private set; }
    public decimal ConveyanceAllowance { get; private set; }
    public string Currency { get; private set; } = "INR";
    public DateTime EffectiveFromUtc { get; private set; }
    public DateTime? EffectiveToUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Employee? Employee { get; private set; }

    private SalaryStructure() { }

    public SalaryStructure(
        Guid employeeId,
        decimal baseSalary,
        DateTime effectiveFrom,
        decimal hra = 0,
        decimal specialAllowance = 0,
        decimal conveyanceAllowance = 0)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        BaseSalary = baseSalary;
        Hra = hra > 0 ? hra : Math.Round(baseSalary * 0.40m, 2);
        SpecialAllowance = specialAllowance > 0 ? specialAllowance : Math.Round(baseSalary * 0.15m, 2);
        ConveyanceAllowance = conveyanceAllowance > 0 ? conveyanceAllowance : 1600m;
        EffectiveFromUtc = effectiveFrom;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateComponents(decimal baseSalary, decimal hra, decimal specialAllowance, decimal conveyance)
    {
        BaseSalary = baseSalary;
        Hra = hra;
        SpecialAllowance = specialAllowance;
        ConveyanceAllowance = conveyance;
        LastModifiedAtUtc = DateTime.UtcNow;
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
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        Amount = amount;
        IsVariable = isVariable;
        SalaryStructureId = salaryStructureId;
        CreatedAtUtc = DateTime.UtcNow;
    }
}

public class Payslip : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public decimal BasicSalary { get; private set; }
    public decimal Hra { get; private set; }
    public decimal Allowances { get; private set; }
    public decimal BonusPay { get; private set; }
    public decimal OvertimePay { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal PfDeduction { get; private set; }
    public decimal EsiDeduction { get; private set; }
    public decimal TdsDeduction { get; private set; }
    public decimal Deductions { get; private set; }
    public decimal ReimbursementsAmount { get; private set; }
    public decimal NetSalary { get; private set; }
    public int WorkingDays { get; private set; } = 30;
    public int PresentDays { get; private set; } = 30;
    public int PaidLeaveDays { get; private set; } = 0;
    public int UnpaidLeaveDays { get; private set; } = 0;
    public string Status { get; private set; } = "Generated"; // Draft, Generated, UnderReview, Approved, Paid
    public string PaymentMethod { get; private set; } = "Direct Bank Transfer";
    public DateTime? PaymentDateUtc { get; private set; }
    public string? Remarks { get; private set; }
    public DateTime GeneratedDateUtc { get; private set; }
    public Employee? Employee { get; private set; }

    private Payslip() { }

    public static Payslip Create(Guid employeeId, int month, int year, decimal gross, decimal deductions)
    {
        var basic = Math.Round(gross * 0.50m, 2);
        var hra = Math.Round(gross * 0.30m, 2);
        var allowances = Math.Max(0, gross - basic - hra);
        var pf = Math.Round(basic * 0.12m, 2);
        var tds = Math.Max(0, deductions - pf);

        return new Payslip
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Month = month,
            Year = year,
            BasicSalary = basic,
            Hra = hra,
            Allowances = allowances,
            GrossSalary = gross,
            PfDeduction = pf,
            TdsDeduction = tds,
            Deductions = deductions,
            NetSalary = gross - deductions,
            Status = "Generated",
            GeneratedDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Payslip CreateDetailed(
        Guid employeeId,
        int month,
        int year,
        decimal basicSalary,
        decimal hra,
        decimal allowances,
        decimal bonusPay,
        decimal overtimePay,
        decimal pfDeduction,
        decimal esiDeduction,
        decimal tdsDeduction,
        decimal reimbursementsAmount,
        int workingDays,
        int presentDays,
        int paidLeaveDays,
        int unpaidLeaveDays,
        string status = "Generated",
        string? remarks = null)
    {
        var gross = basicSalary + hra + allowances + bonusPay + overtimePay;
        var totalDeductions = pfDeduction + esiDeduction + tdsDeduction;
        var net = gross - totalDeductions + reimbursementsAmount;

        return new Payslip
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Month = month,
            Year = year,
            BasicSalary = basicSalary,
            Hra = hra,
            Allowances = allowances,
            BonusPay = bonusPay,
            OvertimePay = overtimePay,
            GrossSalary = gross,
            PfDeduction = pfDeduction,
            EsiDeduction = esiDeduction,
            TdsDeduction = tdsDeduction,
            Deductions = totalDeductions,
            ReimbursementsAmount = reimbursementsAmount,
            NetSalary = net,
            WorkingDays = workingDays,
            PresentDays = presentDays,
            PaidLeaveDays = paidLeaveDays,
            UnpaidLeaveDays = unpaidLeaveDays,
            Status = status,
            Remarks = remarks,
            GeneratedDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkApproved()
    {
        Status = "Approved";
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public void MarkPaid(DateTime paymentDate)
    {
        Status = "Paid";
        PaymentDateUtc = paymentDate;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}
