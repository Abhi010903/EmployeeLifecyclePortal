namespace EmployeeLifecyclePortal.Application.DTOs.Asset;

public class AssetMaintenanceDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? AssetName { get; set; }
    public DateTime MaintenanceDateUtc { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string ServiceProvider { get; set; } = string.Empty;
    
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
