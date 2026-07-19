using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 27: Asset management - Equipment, assignments, returns</summary>
public class Asset : AuditableEntity
{
    public string AssetCode { get; private set; } = string.Empty;
    public string AssetName { get; private set; } = string.Empty;
    public string AssetType { get; private set; } = string.Empty;
    public string SerialNumber { get; private set; } = string.Empty;
    public decimal PurchaseValue { get; private set; }
    public string Status { get; set; } = "Available";
    public string Condition { get; set; } = "Good";
    public DateTime PurchaseDateUtc { get; private set; }

    private Asset() { }

    public Asset(string code, string name, string type, string serial, decimal value, DateTime purchaseDate)
    {
        AssetCode = code;
        AssetName = name;
        AssetType = type;
        SerialNumber = serial;
        PurchaseValue = value;
        PurchaseDateUtc = purchaseDate;
    }

    public void Assign()
    {
        Status = "Assigned";
    }

    public void Retire()
    {
        Status = "Retired";
    }
}

public class AssetAssignment : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid AssetId { get; private set; }
    public DateTime AssignedDateUtc { get; private set; }
    public DateTime? ReturnedDateUtc { get; private set; }
    public string Status { get; private set; } = "Active";
    public string? Notes { get; set; }
    public Employee? Employee { get; private set; }
    public Asset? Asset { get; private set; }

    private AssetAssignment() { }

    public AssetAssignment(Guid employeeId, Guid assetId)
    {
        EmployeeId = employeeId;
        AssetId = assetId;
        AssignedDateUtc = DateTime.UtcNow;
    }

    public void Return()
    {
        Status = "Returned";
        ReturnedDateUtc = DateTime.UtcNow;
    }

    public bool IsActive()
    {
        return Status == "Active" && !ReturnedDateUtc.HasValue;
    }
}

public class AssetMaintenance : AuditableEntity
{
    public Guid AssetId { get; private set; }
    public DateTime MaintenanceDateUtc { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Cost { get; private set; }
    public string ServiceProvider { get; private set; } = string.Empty;
    public Asset? Asset { get; private set; }

    private AssetMaintenance() { }

    public AssetMaintenance(Guid assetId, string description, decimal cost, string provider)
    {
        AssetId = assetId;
        Description = description;
        Cost = cost;
        ServiceProvider = provider;
        MaintenanceDateUtc = DateTime.UtcNow;
    }
}
