namespace Logicware.Connect.Sdk.Models;

public sealed class Manifest
{
    public string Id { get; init; } = string.Empty;
    public string ManifestNumber { get; init; } = string.Empty;
    /// <summary>"Draft", "Finalized", "Shipped", "InTransit", "AtCustoms", "Cleared", "Arrived", "Completed", "Cancelled".</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>"Inbound", "Outbound".</summary>
    public string? Type { get; init; }
    public bool IsOpen { get; init; }
    public int PackageCount { get; init; }
    public decimal? TotalWeightLbs { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }
    public IReadOnlyList<Package>? Packages { get; init; }
}

public class CreateManifestInput
{
    public string Type { get; set; } = "Outbound";
    public string? Description { get; set; }
    public string? WarehouseId { get; set; }
}

public class UpdateManifestInput
{
    public string? Description { get; set; }
}

public class FinalizeManifestInput
{
    public string? Vessel { get; set; }
    public string? VoyageNumber { get; set; }
    public DateTimeOffset? DepartureDate { get; set; }
    public DateTimeOffset? ArrivalDate { get; set; }
    public decimal? CustomsValueUsd { get; set; }
    public string? Notes { get; set; }
}

public class UpdateManifestStatusInput
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
}

public sealed class AddPackagesResult
{
    public int Added { get; init; }
    public int Skipped { get; init; }
    public IReadOnlyList<string>? SkippedReasons { get; init; }
}

public class ListManifestsOptions : PageOptions
{
    public string? Status { get; init; }
    public string? Type { get; init; }
}
