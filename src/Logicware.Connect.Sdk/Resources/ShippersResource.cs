using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

public sealed class ShippersResource : ResourceBase
{
    public ShipperAddressesResource Addresses { get; }

    internal ShippersResource(LogicwareHttpClient http) : base(http)
    {
        Addresses = new ShipperAddressesResource(http);
    }

    public Task<PaginatedResponse<Shipper>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default) =>
        GetPagedAsync<Shipper>(
            new RequestDescriptor { Method = "GET", Path = "/api/v1/shippers", Query = BuildPageQuery(options) },
            cancellationToken);

    /// <summary>Async iterator that transparently follows pagination.</summary>
    public IAsyncEnumerable<Shipper> ListAllAsync(PageOptions? options = null, CancellationToken cancellationToken = default) =>
        PaginateAsync<Shipper>(
            page =>
            {
                var q = BuildPageQuery(options);
                q["page"] = page.ToString();
                return new RequestDescriptor { Method = "GET", Path = "/api/v1/shippers", Query = q };
            },
            cancellationToken);

    public Task<Shipper> GetAsync(string id, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Shipper>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/shippers/{Escape(id)}" },
            cancellationToken);

    public Task<Shipper> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Shipper>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/shippers/by-email/{Escape(email)}" },
            cancellationToken);

    public Task<Shipper> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Shipper>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/shippers/by-code/{Escape(code)}" },
            cancellationToken);

    public Task<Shipper> CreateAsync(CreateShipperInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Shipper>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/shippers", Body = input },
            cancellationToken);

    public Task<Shipper> UpdateAsync(string id, UpdateShipperInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Shipper>(
            new RequestDescriptor { Method = "PUT", Path = $"/api/v1/shippers/{Escape(id)}", Body = input },
            cancellationToken);

    /// <summary>
    /// Upsert-by-email. Safe to replay — the server matches on normalized
    /// email and updates instead of duplicating. <see cref="SyncShipperResult.Status"/>
    /// is <c>"created"</c>, <c>"updated"</c>, or <c>"skipped"</c>.
    /// </summary>
    public Task<SyncShipperResult> SyncAsync(CreateShipperInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<SyncShipperResult>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/shippers/sync", Body = input },
            cancellationToken);

    /// <summary>
    /// Synchronous bulk create for small batches (up to 500). Larger batches
    /// should use <see cref="ImportManyAsync"/>. The API chunks internally
    /// but still returns every row result.
    /// </summary>
    public async Task<BulkShippersResponse> BulkCreateAsync(
        IReadOnlyList<CreateShipperInput> inputs,
        CancellationToken cancellationToken = default)
    {
        var envelope = await Http.RequestAsync<DataEnvelope<BulkShippersResponse>>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/shippers/bulk", Body = new { shippers = inputs } },
            cancellationToken).ConfigureAwait(false);
        return envelope?.Data ?? new BulkShippersResponse();
    }

    /// <summary>
    /// Queue an async import (up to 100k rows). Returns a job id; poll
    /// <see cref="GetImportAsync"/> or iterate <see cref="ImportProgressAsync"/>.
    /// </summary>
    public async Task<string> ImportManyAsync(
        IReadOnlyList<CreateShipperInput> inputs,
        CancellationToken cancellationToken = default)
    {
        var envelope = await Http.RequestAsync<DataEnvelope<ImportAccepted>>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/shippers/imports", Body = new { shippers = inputs } },
            cancellationToken).ConfigureAwait(false);
        return envelope?.Data?.JobId
            ?? throw new InvalidOperationException("Import endpoint returned an empty job id");
    }

    public Task<ShipperImport> GetImportAsync(string jobId, CancellationToken cancellationToken = default) =>
        GetSingleAsync<ShipperImport>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/shippers/imports/{Escape(jobId)}" },
            cancellationToken);

    public Task<PaginatedResponse<BulkShipperResult>> GetImportFailuresAsync(
        string jobId,
        PageOptions? options = null,
        CancellationToken cancellationToken = default) =>
        GetPagedAsync<BulkShipperResult>(
            new RequestDescriptor
            {
                Method = "GET",
                Path = $"/api/v1/shippers/imports/{Escape(jobId)}/failures",
                Query = BuildPageQuery(options),
            },
            cancellationToken);

    /// <summary>
    /// Poll an import job until it terminates. Yields the current snapshot
    /// on each tick. Default poll interval is 2 seconds.
    /// </summary>
    public async IAsyncEnumerable<ShipperImport> ImportProgressAsync(
        string jobId,
        TimeSpan? pollInterval = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var interval = pollInterval ?? TimeSpan.FromSeconds(2);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = await GetImportAsync(jobId, cancellationToken).ConfigureAwait(false);
            yield return snapshot;
            if (snapshot.Status is "Completed" or "PartialSuccess" or "Failed")
            {
                yield break;
            }
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class ImportAccepted
    {
        public string JobId { get; init; } = string.Empty;
    }
}

public sealed class ShipperAddressesResource : ResourceBase
{
    internal ShipperAddressesResource(LogicwareHttpClient http) : base(http) { }

    public async Task<IReadOnlyList<ShipperAddress>> ListAsync(string shipperId, CancellationToken cancellationToken = default)
    {
        var envelope = await Http.RequestAsync<DataEnvelope<IReadOnlyList<ShipperAddress>>>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/shippers/{Escape(shipperId)}/addresses" },
            cancellationToken).ConfigureAwait(false);
        return envelope?.Data ?? Array.Empty<ShipperAddress>();
    }

    public Task<ShipperAddress> CreateAsync(string shipperId, CreateShipperAddressInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<ShipperAddress>(
            new RequestDescriptor { Method = "POST", Path = $"/api/v1/shippers/{Escape(shipperId)}/addresses", Body = input },
            cancellationToken);

    public Task<ShipperAddress> UpdateAsync(string shipperId, string addressId, UpdateShipperAddressInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<ShipperAddress>(
            new RequestDescriptor { Method = "PATCH", Path = $"/api/v1/shippers/{Escape(shipperId)}/addresses/{Escape(addressId)}", Body = input },
            cancellationToken);

    /// <summary>Soft-delete. Optional <paramref name="reason"/> is recorded on the audit trail.</summary>
    public Task DeleteAsync(string shipperId, string addressId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(reason)) query["reason"] = reason;
        return Http.RequestAsync(
            new RequestDescriptor
            {
                Method = "DELETE",
                Path = $"/api/v1/shippers/{Escape(shipperId)}/addresses/{Escape(addressId)}",
                Query = query,
            },
            cancellationToken);
    }
}
