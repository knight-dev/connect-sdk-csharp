using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

public sealed class PackagesResource : ResourceBase
{
    internal PackagesResource(LogicwareHttpClient http) : base(http) { }

    public Task<PaginatedResponse<Package>> ListAsync(ListPackagesOptions? options = null, CancellationToken cancellationToken = default) =>
        GetPagedAsync<Package>(BuildListDescriptor(options), cancellationToken);

    public IAsyncEnumerable<Package> ListAllAsync(ListPackagesOptions? options = null, CancellationToken cancellationToken = default) =>
        PaginateAsync<Package>(
            page =>
            {
                var d = BuildListDescriptor(options);
                var q = new Dictionary<string, string?>(d.Query!);
                q["page"] = page.ToString();
                return new RequestDescriptor { Method = d.Method, Path = d.Path, Query = q };
            },
            cancellationToken);

    public Task<Package> GetAsync(string id, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Package>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/packages/{Escape(id)}" },
            cancellationToken);

    /// <summary>
    /// Public tracking lookup. No auth required — works for customers who
    /// only have the carrier tracking number.
    /// </summary>
    public Task<Package> GetByTrackingAsync(string trackingNumber, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Package>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/packages/track/{Escape(trackingNumber)}" },
            cancellationToken);

    public Task<Package> CreateAsync(CreatePackageInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Package>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/packages", Body = input },
            cancellationToken);

    public Task<Package> UpdateAsync(string id, UpdatePackageInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Package>(
            new RequestDescriptor { Method = "PUT", Path = $"/api/v1/packages/{Escape(id)}", Body = input },
            cancellationToken);

    public Task<PaginatedResponse<Package>> ForShipperAsync(string shipperId, PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var opts = new ListPackagesOptions
        {
            ShipperId = shipperId,
            Page = options?.Page,
            PageSize = options?.PageSize,
            Search = options?.Search,
        };
        return ListAsync(opts, cancellationToken);
    }

    public Task<PaginatedResponse<Package>> ForManifestAsync(string manifestId, PageOptions? options = null, CancellationToken cancellationToken = default) =>
        GetPagedAsync<Package>(
            new RequestDescriptor
            {
                Method = "GET",
                Path = $"/api/v1/manifests/{Escape(manifestId)}/packages",
                Query = BuildPageQuery(options),
            },
            cancellationToken);

    private static RequestDescriptor BuildListDescriptor(ListPackagesOptions? options)
    {
        var query = BuildPageQuery(options);
        if (options?.ShipperId is string sid) query["shipperId"] = sid;
        if (options?.Status is string status) query["status"] = status;
        return new RequestDescriptor { Method = "GET", Path = "/api/v1/packages", Query = query };
    }
}
