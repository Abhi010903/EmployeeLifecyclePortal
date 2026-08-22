namespace EmployeeLifecyclePortal.Application.DTOs.Payroll;

public sealed class SalaryStructureDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Hra { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public string Currency { get; set; } = "INR";
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}

public sealed class SalaryComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Earning";
    public decimal Amount { get; set; }
    public bool IsVariable { get; set; }
    public Guid SalaryStructureId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}

public sealed class PayslipDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? DepartmentName { get; set; }
    public string? RoleName { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Hra { get; set; }
    public decimal Allowances { get; set; }
    public decimal BonusPay { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal PfDeduction { get; set; }
    public decimal EsiDeduction { get; set; }
    public decimal TdsDeduction { get; set; }
    public decimal Deductions { get; set; }
    public decimal ReimbursementsAmount { get; set; }
    public decimal NetSalary { get; set; }
    public int WorkingDays { get; set; } = 30;
    public int PresentDays { get; set; } = 30;
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public string Status { get; set; } = "Generated";
    public string PaymentMethod { get; set; } = "Direct Bank Transfer";
    public DateTime? PaymentDateUtc { get; set; }
    public string? Remarks { get; set; }
    public DateTime GeneratedDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}

public sealed class ReimbursementDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
    public string Status { get; set; } = "Pending";
    public Guid? ApprovedByUserId { get; private set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? RejectionReason { get; set; }
    public string? PayrollPeriod { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
}

public sealed class PayrollSummaryDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalEmployees { get; set; }
    public int ProcessedCount { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalPf { get; set; }
    public decimal TotalEsi { get; set; }
    public decimal TotalTds { get; set; }
    public decimal TotalReimbursements { get; set; }
    public decimal TotalNetSalary { get; set; }
    public string Status { get; set; } = "UnderReview";
    public List<DepartmentPayrollDto> DepartmentBreakdown { get; set; } = new();
    public List<PayslipDto> Payslips { get; set; } = new();
}

public sealed class DepartmentPayrollDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
}
