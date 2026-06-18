namespace EmployeeLifecyclePortal.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; protected set; }

    public string? CreatedBy { get; protected set; }

    public DateTime? LastModifiedAtUtc { get; protected set; }

    public string? LastModifiedBy { get; protected set; }

    protected AuditableEntity()
    {
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void SetCreationAudit(
        string createdBy)
    {
        CreatedBy = createdBy;
    }

    public void SetModificationAudit(
        string modifiedBy)
    {
        LastModifiedAtUtc = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}