namespace Logicware.Connect.Sdk.Models;

public sealed class Warehouse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? AddressPrefix { get; init; }
    public IReadOnlyList<string> FreightTypes { get; init; } = Array.Empty<string>();
    public string? TimeZoneId { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
}
