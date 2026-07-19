namespace EmployeeLifecyclePortal.Application.DTOs.Asset;

public class AssetDto
{
    public Guid Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public decimal PurchaseValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public DateTime PurchaseDateUtc { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
