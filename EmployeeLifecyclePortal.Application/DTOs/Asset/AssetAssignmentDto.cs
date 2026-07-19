namespace EmployeeLifecyclePortal.Application.DTOs.Asset;

public class AssetAssignmentDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid AssetId { get; set; }
    public string? AssetName { get; set; }
    public string? AssetType { get; set; }
    public DateTime AssignedDateUtc { get; set; }
    public DateTime? ReturnedDateUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
