using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

public sealed class ManifestsResource : ResourceBase
{
    internal ManifestsResource(LogicwareHttpClient http) : base(http) { }

    public Task<PaginatedResponse<Manifest>> ListAsync(ListManifestsOptions? options = null, CancellationToken cancellationToken = default) =>
        GetPagedAsync<Manifest>(BuildListDescriptor(options), cancellationToken);

    public IAsyncEnumerable<Manifest> ListAllAsync(ListManifestsOptions? options = null, CancellationToken cancellationToken = default) =>
        PaginateAsync<Manifest>(
            page =>
            {
                var d = BuildListDescriptor(options);
                var q = new Dictionary<string, string?>(d.Query!);
                q["page"] = page.ToString();
                return new RequestDescriptor { Method = d.Method, Path = d.Path, Query = q };
            },
            cancellationToken);

    public Task<Manifest> GetAsync(string id, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Manifest>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/manifests/{Escape(id)}" },
            cancellationToken);

    public Task<Manifest> CreateAsync(CreateManifestInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Manifest>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/manifests", Body = input },
            cancellationToken);

    public Task<Manifest> UpdateAsync(string id, UpdateManifestInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Manifest>(
            new RequestDescriptor { Method = "PUT", Path = $"/api/v1/manifests/{Escape(id)}", Body = input },
            cancellationToken);

    /// <summary>
    /// Set <c>IsOpen</c>. <c>isOpen: true</c> auto-closes any other open
    /// manifest of the same type. <c>isOpen: false</c> stops auto-linking
    /// — NOT the same as <see cref="FinalizeAsync"/>.
    /// </summary>
    public Task<Manifest> SetOpenAsync(string id, bool isOpen, bool autoLinkPackages = false, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Manifest>(
            new RequestDescriptor
            {
                Method = "POST",
                Path = $"/api/v1/manifests/{Escape(id)}/open",
                Body = new { isOpen, autoLinkPackages },
            },
            cancellationToken);

    public Task<Manifest> CloseAsync(string id, CancellationToken cancellationToken = default) =>
        SetOpenAsync(id, isOpen: false, autoLinkPackages: false, cancellationToken);

    public Task<Manifest> ReopenAsync(string id, bool autoLinkPackages = false, CancellationToken cancellationToken = default) =>
        SetOpenAsync(id, isOpen: true, autoLinkPackages, cancellationToken);

    /// <summary>Transition Draft → Finalized. Captures customs + financial data.</summary>
    public Task<Manifest> FinalizeAsync(string id, FinalizeManifestInput? input = null, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Manifest>(
            new RequestDescriptor { Method = "POST", Path = $"/api/v1/manifests/{Escape(id)}/finalize", Body = input ?? new FinalizeManifestInput() },
            cancellationToken);

    /// <summary>
    /// Post-finalize status transition:
    /// Finalized → Shipped → InTransit → AtCustoms → Cleared → Arrived → Completed
    /// (or Cancelled from any non-Completed state).
    /// </summary>
    public Task<Manifest> SetStatusAsync(string id, UpdateManifestStatusInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Manifest>(
            new RequestDescriptor { Method = "POST", Path = $"/api/v1/manifests/{Escape(id)}/status", Body = input },
            cancellationToken);

    public Task<AddPackagesResult> AddPackagesAsync(string id, IEnumerable<string> packageIds, CancellationToken cancellationToken = default) =>
        GetSingleAsync<AddPackagesResult>(
            new RequestDescriptor
            {
                Method = "POST",
                Path = $"/api/v1/manifests/{Escape(id)}/packages",
                Body = new { packageIds },
            },
            cancellationToken);

    public Task<Manifest> RemovePackageAsync(string id, string packageId, CancellationToken cancellationToken = default) =>
        GetSingleAsync<Manifest>(
            new RequestDescriptor
            {
                Method = "DELETE",
                Path = $"/api/v1/manifests/{Escape(id)}/packages/{Escape(packageId)}",
            },
            cancellationToken);

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        Http.RequestAsync(
            new RequestDescriptor { Method = "DELETE", Path = $"/api/v1/manifests/{Escape(id)}" },
            cancellationToken);

    private static RequestDescriptor BuildListDescriptor(ListManifestsOptions? options)
    {
        var query = BuildPageQuery(options);
        if (options?.Status is string status) query["status"] = status;
        if (options?.Type is string type) query["type"] = type;
        return new RequestDescriptor { Method = "GET", Path = "/api/v1/manifests", Query = query };
    }
}
