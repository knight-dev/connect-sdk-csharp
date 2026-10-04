using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

public sealed class MissingPackagesResource : ResourceBase
{
    internal MissingPackagesResource(LogicwareHttpClient http) : base(http) { }

    public Task<PaginatedResponse<MissingPackageRequest>> ListAsync(
        ListMissingPackagesOptions? options = null,
        CancellationToken cancellationToken = default) =>
        GetPagedAsync<MissingPackageRequest>(BuildListDescriptor(options), cancellationToken);

    public IAsyncEnumerable<MissingPackageRequest> ListAllAsync(
        ListMissingPackagesOptions? options = null,
        CancellationToken cancellationToken = default) =>
        PaginateAsync<MissingPackageRequest>(
            page =>
            {
                var d = BuildListDescriptor(options);
                var q = new Dictionary<string, string?>(d.Query!);
                q["page"] = page.ToString();
                return new RequestDescriptor { Method = d.Method, Path = d.Path, Query = q };
            },
            cancellationToken);

    public Task<MissingPackageRequest> GetAsync(string id, CancellationToken cancellationToken = default) =>
        GetSingleAsync<MissingPackageRequest>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/missing-packages/{Escape(id)}" },
            cancellationToken);

    public Task<CreateMissingPackageResult> CreateAsync(CreateMissingPackageInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<CreateMissingPackageResult>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/missing-packages", Body = input },
            cancellationToken);

    public Task CancelAsync(string id, string? reason = null, CancellationToken cancellationToken = default) =>
        Http.RequestAsync(
            new RequestDescriptor
            {
                Method = "POST",
                Path = $"/api/v1/missing-packages/{Escape(id)}/cancel",
                Body = reason is null ? null : new { reason },
            },
            cancellationToken);

    public Task CloseAsync(string id, string? resolutionNotes = null, CancellationToken cancellationToken = default) =>
        Http.RequestAsync(
            new RequestDescriptor
            {
                Method = "POST",
                Path = $"/api/v1/missing-packages/{Escape(id)}/close",
                Body = resolutionNotes is null ? null : new { resolutionNotes },
            },
            cancellationToken);

    private static RequestDescriptor BuildListDescriptor(ListMissingPackagesOptions? options)
    {
        var query = BuildPageQuery(options);
        if (options?.Status is string status) query["status"] = status;
        if (options?.Priority is string priority) query["priority"] = priority;
        return new RequestDescriptor { Method = "GET", Path = "/api/v1/missing-packages", Query = query };
    }
}
