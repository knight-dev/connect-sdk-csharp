namespace Logicware.Connect.Sdk.Models;

public sealed class PreAlert
{
    public string Id { get; init; } = string.Empty;
    public string? ShipperId { get; init; }
    public string? ShipperAddressCode { get; init; }
    public string? CarrierTrackingNumber { get; init; }
    public string? Carrier { get; init; }
    public string? Description { get; init; }
    public string? MerchantName { get; init; }
    public decimal? ExpectedWeightLbs { get; init; }
    public decimal? DeclaredValueUsd { get; init; }
    /// <summary>"Pending", "Matched", "Expired", "Cancelled".</summary>
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public string? MatchedPackageId { get; init; }
}

public sealed class PreAlertStats
{
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Matched { get; init; }
    public int Expired { get; init; }
}

public sealed class PreAlertListResponse
{
    public IReadOnlyList<PreAlert> Data { get; init; } = Array.Empty<PreAlert>();
    public PaginationMeta Pagination { get; init; } = new();
    public PreAlertStats? Stats { get; init; }
}

public class CreatePreAlertInput
{
    public string ShipperAddressCode { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public string? Description { get; set; }
    public string? MerchantName { get; set; }
    public decimal? ExpectedWeightLbs { get; set; }
    public decimal? DeclaredValueUsd { get; set; }
}

public sealed class CreatePreAlertResult
{
    public string Id { get; init; } = string.Empty;
    public string? ShipperAddressCode { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class LookupPreAlertResult
{
    public bool Found { get; init; }
    public PreAlert? Data { get; init; }
}

public class ListPreAlertsOptions : PageOptions
{
    public string? Status { get; init; }
}
