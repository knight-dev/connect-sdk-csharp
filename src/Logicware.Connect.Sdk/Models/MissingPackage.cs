namespace Logicware.Connect.Sdk.Models;

public sealed class MissingPackageRequest
{
    public string Id { get; init; } = string.Empty;
    public string? ShipperId { get; init; }
    public string? ShipperName { get; init; }
    public string? Description { get; init; }
    public string? TrackingNumber { get; init; }
    public string? MerchantName { get; init; }
    public decimal? DeclaredValueUsd { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Priority { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
    public string? ResolutionNotes { get; init; }
}

public class CreateMissingPackageInput
{
    public string ShipperId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TrackingNumber { get; set; }
    public string? MerchantName { get; set; }
    public decimal? DeclaredValueUsd { get; set; }
    public string? Priority { get; set; }
}

public sealed class CreateMissingPackageResult
{
    public string Id { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public class ListMissingPackagesOptions : PageOptions
{
    public string? Status { get; init; }
    public string? Priority { get; init; }
}
