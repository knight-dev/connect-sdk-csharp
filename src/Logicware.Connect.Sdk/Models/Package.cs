namespace Logicware.Connect.Sdk.Models;

public sealed class Package
{
    public string Id { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public string? InternalBarcode { get; init; }
    public string Status { get; init; } = string.Empty;
    /// <summary>"Air" or "Sea".</summary>
    public string? FreightType { get; init; }
    /// <summary>Box / Bag / Envelope / Tube / Crate / Pallet / Irregular / Other.</summary>
    public string? PackageType { get; init; }
    /// <summary>Good / MinorDamage / ModerateDamage / SevereDamage / Tampered / WetDamaged / Fragile.</summary>
    public string? Condition { get; init; }
    public string? ConditionNotes { get; init; }
    public string? Description { get; init; }
    public decimal? WeightLbs { get; init; }
    public decimal? LengthIn { get; init; }
    public decimal? WidthIn { get; init; }
    public decimal? HeightIn { get; init; }
    /// <summary>L×W×H/139 — chargeable when bigger than actual weight.</summary>
    public decimal? DimensionalWeightLbs { get; init; }
    /// <summary>max(WeightLbs, DimensionalWeightLbs) — what the courier bills against.</summary>
    public decimal? BillableWeightLbs { get; init; }
    public decimal? DeclaredValueUsd { get; init; }
    public decimal? TotalDueJmd { get; init; }
    public string? ShipperId { get; init; }
    public string? ShipperName { get; init; }
    public string? ShipperAddressCode { get; init; }
    public string? ManifestId { get; init; }
    public string? ManifestNumber { get; init; }
    /// <summary>Marketplace where the package originated (Amazon, Shein, etc.).</summary>
    public string? SourceMarketplace { get; init; }
    public string? MerchantName { get; init; }
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
    /// <summary>Box / Bag / Envelope / Tube / Crate / Pallet / Irregular / Other.</summary>
    public string? PackageType { get; set; }
    public string? SourceMarketplace { get; set; }
    public string? MerchantName { get; set; }
}

public class UpdatePackageInput
{
    public string? Description { get; set; }
    public decimal? WeightLbs { get; set; }
    public decimal? LengthIn { get; set; }
    public decimal? WidthIn { get; set; }
    public decimal? HeightIn { get; set; }
    public decimal? DeclaredValueUsd { get; set; }
    public string? Status { get; set; }
    public string? FreightType { get; set; }
    public string? PackageType { get; set; }
    public string? Condition { get; set; }
    public string? ConditionNotes { get; set; }
    public string? SourceMarketplace { get; set; }
    public string? MerchantName { get; set; }
}

public class ListPackagesOptions : PageOptions
{
    public string? ShipperId { get; init; }
    public string? Status { get; init; }
}
