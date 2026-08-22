using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>
/// Reimbursement claim submitted by an employee for business expenses.
/// Integrates with the Payroll module upon approval.
/// </summary>
public class Reimbursement : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public decimal Amount { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? ReceiptUrl { get; private set; }
    public string Status { get; private set; } = "Pending"; // Pending, Approved, Rejected, Paid
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? PayrollPeriod { get; private set; } // e.g. "2026-08"

    public Employee? Employee { get; private set; }

    private Reimbursement() { }

    public Reimbursement(
        Guid employeeId,
        decimal amount,
        string category,
        string description,
        string? receiptUrl = null)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        Amount = amount;
        Category = category;
        Description = description;
        ReceiptUrl = receiptUrl;
        Status = "Pending";
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Approve(Guid approvedByUserId)
    {
        Status = "Approved";
        ApprovedByUserId = approvedByUserId;
        ApprovedAtUtc = DateTime.UtcNow;
        RejectionReason = null;
    }

    public void Reject(Guid rejectedByUserId, string reason)
    {
        Status = "Rejected";
        ApprovedByUserId = rejectedByUserId;
        ApprovedAtUtc = DateTime.UtcNow;
        RejectionReason = reason;
    }

    public void MarkPaid(string payrollPeriod)
    {
        Status = "Paid";
        PayrollPeriod = payrollPeriod;
    }
}
