namespace Logicware.Connect.Sdk.Models;

public sealed class Package
{
    public string Id { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public string? InternalBarcode { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FreightType { get; init; }
    public string? Description { get; init; }
    public decimal? WeightLbs { get; init; }
    public decimal? DeclaredValueUsd { get; init; }
    public decimal? TotalDueJmd { get; init; }
    public string? ShipperId { get; init; }
    public string? ShipperName { get; init; }
    public string? ShipperAddressCode { get; init; }
    public string? ManifestId { get; init; }
    public string? ManifestNumber { get; init; }
    public DateTimeOffset? ReceivedAt { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? DeliveredAt { get; init; }
}

public class CreatePackageInput
{
    public string? ShipperId { get; set; }
    public string? ShipperAddressCode { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Description { get; set; }
    public decimal? WeightLbs { get; set; }
    public decimal? DeclaredValueUsd { get; set; }
    public string? FreightType { get; set; }
}

public class UpdatePackageInput
{
    public string? Description { get; set; }
    public decimal? WeightLbs { get; set; }
    public decimal? DeclaredValueUsd { get; set; }
    public string? Status { get; set; }
    public string? FreightType { get; set; }
}

public class ListPackagesOptions : PageOptions
{
    public string? ShipperId { get; init; }
    public string? Status { get; init; }
}
