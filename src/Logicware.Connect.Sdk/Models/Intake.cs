namespace Logicware.Connect.Sdk.Models;

public sealed class IntakePackage
{
    public string Id { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public string? Description { get; init; }
    public decimal? WeightLbs { get; init; }
    public decimal? DeclaredValueUsd { get; init; }
    public string? AddressCode { get; init; }
    public string? ShipperId { get; init; }
    public string? ShipperName { get; init; }
    public string? CustomerName { get; init; }
    public string? WarehouseCode { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? ReceivedAt { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}

public class SearchUnidentifiedOptions
{
    public string? TrackingNumber { get; init; }
    public string? CustomerName { get; init; }
    public int? Limit { get; init; }
}

public class ListUnclaimedOptions : PageOptions
{
}

public class ListReceivedOptions : PageOptions
{
    public DateTimeOffset Since { get; init; }
}
