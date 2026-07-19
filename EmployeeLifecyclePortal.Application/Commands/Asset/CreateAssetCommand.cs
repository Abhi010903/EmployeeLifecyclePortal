using MediatR;
using EmployeeLifecyclePortal.Application.DTOs.Asset;

namespace EmployeeLifecyclePortal.Application.Commands.Asset;

public sealed class CreateAssetCommand : IRequest<AssetDto>
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public decimal PurchaseValue { get; set; }
    public DateTime PurchaseDateUtc { get; set; }

    public CreateAssetCommand(string code, string name, string type, string serial, decimal value, DateTime purchaseDate)
    {
        AssetCode = code;
        AssetName = name;
        AssetType = type;
        SerialNumber = serial;
        PurchaseValue = value;
        PurchaseDateUtc = purchaseDate;
    }
}
