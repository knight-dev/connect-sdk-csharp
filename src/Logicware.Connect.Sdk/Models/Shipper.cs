namespace Logicware.Connect.Sdk.Models;

public sealed class Shipper
{
    public string Id { get; init; } = string.Empty;
    public string? ShipperCode { get; init; }
    public string? AddressCode { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public int? TotalPackages { get; init; }
    public string? Trn { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public IReadOnlyList<ShipperAddress>? Addresses { get; init; }
    public decimal? TotalSpentJmd { get; init; }
    public decimal? OutstandingBalanceJmd { get; init; }
    public int? UnpaidInvoicesCount { get; init; }
}

public sealed class ShipperAddress
{
    public string Id { get; init; } = string.Empty;
    public string AddressCode { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}

public class CreateShipperInput
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Trn { get; set; }
    public string? AddressCode { get; set; }
    public string? WarehouseId { get; set; }
    public string? FreightType { get; set; }
    public bool? GenerateAddressCode { get; set; }
    public bool? ForceAddressCode { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Parish { get; set; }
    public string? PostalCode { get; set; }
}

public class UpdateShipperInput
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Trn { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SyncShipperResult
{
    /// <summary>"created", "updated", or "skipped".</summary>
    public string Status { get; init; } = string.Empty;
    public string ShipperId { get; init; } = string.Empty;
    public string? AddressCode { get; init; }
    public string? AddressOutcome { get; init; }
}

public sealed class BulkShipperResult
{
    public int Index { get; init; }
    public string? Email { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ShipperId { get; init; }
    public string? AddressCode { get; init; }
    public string? AddressOutcome { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class BulkShippersResponse
{
    public IReadOnlyList<BulkShipperResult> Results { get; init; } = Array.Empty<BulkShipperResult>();
    public int TotalRows { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
}

public sealed class ShipperImport
{
    public string Id { get; init; } = string.Empty;
    /// <summary>"Queued", "Running", "Completed", "PartialSuccess", "Failed".</summary>
    public string Status { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int ProcessedRows { get; init; }
    public int SuccessRows { get; init; }
    public int FailedRows { get; init; }
    public DateTimeOffset? QueuedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? JobErrorMessage { get; init; }
}

public class CreateShipperAddressInput
{
    public string? AddressCode { get; set; }
    public string? WarehouseId { get; set; }
    public string? FreightType { get; set; }
    public bool? GenerateAddressCode { get; set; }
    public bool? IsPrimary { get; set; }
}

public class UpdateShipperAddressInput
{
    public bool? IsPrimary { get; set; }
    public bool? IsActive { get; set; }
}
