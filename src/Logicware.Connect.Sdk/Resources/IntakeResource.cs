using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

public sealed class IntakeResource : ResourceBase
{
    internal IntakeResource(LogicwareHttpClient http) : base(http) { }

    /// <summary>
    /// Search packages that arrived at the warehouse but couldn't be matched
    /// to a shipper automatically. At least one of <paramref name="options"/>
    /// fields must be set; capped at 20 matches server-side.
    /// </summary>
    public async Task<IReadOnlyList<IntakePackage>> SearchUnidentifiedAsync(
        SearchUnidentifiedOptions options,
        CancellationToken cancellationToken = default)
    {
        var q = new Dictionary<string, string?>();
        if (options.TrackingNumber is string t) q["trackingNumber"] = t;
        if (options.CustomerName is string c) q["customerName"] = c;
        if (options.Limit is int l) q["limit"] = l.ToString();
        var envelope = await Http.RequestAsync<DataEnvelope<IReadOnlyList<IntakePackage>>>(
            new RequestDescriptor { Method = "GET", Path = "/api/v1/intake/unidentified/search", Query = q },
            cancellationToken).ConfigureAwait(false);
        return envelope?.Data ?? Array.Empty<IntakePackage>();
    }

    public Task<PaginatedResponse<IntakePackage>> ListUnclaimedAsync(
        ListUnclaimedOptions? options = null,
        CancellationToken cancellationToken = default) =>
        GetPagedAsync<IntakePackage>(
            new RequestDescriptor
            {
                Method = "GET",
                Path = "/api/v1/intake/unclaimed",
                Query = BuildPageQuery(options),
            },
            cancellationToken);

    public IAsyncEnumerable<IntakePackage> ListAllUnclaimedAsync(
        ListUnclaimedOptions? options = null,
        CancellationToken cancellationToken = default) =>
        PaginateAsync<IntakePackage>(
            page =>
            {
                var q = BuildPageQuery(options);
                q["page"] = page.ToString();
                return new RequestDescriptor { Method = "GET", Path = "/api/v1/intake/unclaimed", Query = q };
            },
            cancellationToken);

    public Task<PaginatedResponse<IntakePackage>> ListReceivedAsync(
        ListReceivedOptions options,
        CancellationToken cancellationToken = default)
    {
        var q = BuildPageQuery(options);
        q["since"] = options.Since.ToUniversalTime().ToString("O");
        return GetPagedAsync<IntakePackage>(
            new RequestDescriptor { Method = "GET", Path = "/api/v1/intake/received", Query = q },
            cancellationToken);
    }

    public IAsyncEnumerable<IntakePackage> ListAllReceivedAsync(
        ListReceivedOptions options,
        CancellationToken cancellationToken = default) =>
        PaginateAsync<IntakePackage>(
            page =>
            {
                var q = BuildPageQuery(options);
                q["since"] = options.Since.ToUniversalTime().ToString("O");
                q["page"] = page.ToString();
                return new RequestDescriptor { Method = "GET", Path = "/api/v1/intake/received", Query = q };
            },
            cancellationToken);
}
