using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

public sealed class WarehousesResource : ResourceBase
{
    internal WarehousesResource(LogicwareHttpClient http) : base(http) { }

    /// <summary>
    /// Every warehouse this courier is linked to (address prefix, freight
    /// types, timezone). Not paginated.
    /// </summary>
    public async Task<IReadOnlyList<Warehouse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var envelope = await Http.RequestAsync<DataEnvelope<IReadOnlyList<Warehouse>>>(
            new RequestDescriptor { Method = "GET", Path = "/api/v1/warehouses" },
            cancellationToken).ConfigureAwait(false);
        return envelope?.Data ?? Array.Empty<Warehouse>();
    }

    public Task<Warehouse> GetAsync(string id, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Warehouse>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/warehouses/{Escape(id)}" },
            cancellationToken);
}
