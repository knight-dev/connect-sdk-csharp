using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

public sealed class PreAlertsResource : ResourceBase
{
    internal PreAlertsResource(LogicwareHttpClient http) : base(http) { }

    /// <summary>
    /// Paginated list with courier-scoped stats (<see cref="PreAlertStats"/>).
    /// </summary>
    public async Task<PreAlertListResponse> ListAsync(ListPreAlertsOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = await Http.RequestAsync<PreAlertListResponse>(BuildListDescriptor(options), cancellationToken).ConfigureAwait(false);
        return result ?? new PreAlertListResponse();
    }

    public async IAsyncEnumerable<PreAlert> ListAllAsync(
        ListPreAlertsOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        while (true)
        {
            var d = BuildListDescriptor(options);
            var q = new Dictionary<string, string?>(d.Query!);
            q["page"] = page.ToString();
            var descriptor = new RequestDescriptor { Method = d.Method, Path = d.Path, Query = q };
            var response = await ListAsync(
                new ListPreAlertsOptions { Page = page, PageSize = options?.PageSize, Status = options?.Status },
                cancellationToken).ConfigureAwait(false);
            foreach (var item in response.Data) yield return item;
            if (response.Pagination.TotalPages <= 0 || response.Pagination.Page >= response.Pagination.TotalPages)
            {
                yield break;
            }
            page++;
        }
    }

    public Task<PreAlert> GetAsync(string id, CancellationToken cancellationToken = default) =>
        GetSingleAsync<PreAlert>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/prealerts/{Escape(id)}" },
            cancellationToken);

    /// <summary>
    /// <c>{ found, data }</c> envelope. Returns <c>Found = false</c> with
    /// <c>Data = null</c> when no pre-alert matches — doesn't throw.
    /// </summary>
    public async Task<LookupPreAlertResult> LookupByTrackingAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        var result = await Http.RequestAsync<LookupPreAlertResult>(
            new RequestDescriptor
            {
                Method = "GET",
                Path = "/api/v1/prealerts/lookup",
                Query = new Dictionary<string, string?> { ["tracking"] = trackingNumber },
            },
            cancellationToken).ConfigureAwait(false);
        return result ?? new LookupPreAlertResult();
    }

    public Task<CreatePreAlertResult> CreateAsync(CreatePreAlertInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<CreatePreAlertResult>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/prealerts", Body = input },
            cancellationToken);

    public Task CancelAsync(string id, CancellationToken cancellationToken = default) =>
        Http.RequestAsync(
            new RequestDescriptor { Method = "POST", Path = $"/api/v1/prealerts/{Escape(id)}/cancel" },
            cancellationToken);

    private static RequestDescriptor BuildListDescriptor(ListPreAlertsOptions? options)
    {
        var query = BuildPageQuery(options);
        if (options?.Status is string status) query["status"] = status;
        return new RequestDescriptor { Method = "GET", Path = "/api/v1/prealerts", Query = query };
    }
}
