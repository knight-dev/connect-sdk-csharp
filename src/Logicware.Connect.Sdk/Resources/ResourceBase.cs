using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

/// <summary>
/// Common building blocks every resource uses — URL escaping, page-query
/// construction, single-item unwrapping from <c>{ success, data }</c>.
/// </summary>
public abstract class ResourceBase
{
    protected LogicwareHttpClient Http { get; }

    protected ResourceBase(LogicwareHttpClient http)
    {
        Http = http;
    }

    protected static string Escape(string segment) => Uri.EscapeDataString(segment);

    /// <summary>
    /// Build a query dictionary with standard page/pageSize/search keys.
    /// Extra keys can be appended by the caller.
    /// </summary>
    protected static Dictionary<string, string?> BuildPageQuery(PageOptions? options)
    {
        var query = new Dictionary<string, string?>();
        if (options is null) return query;
        if (options.Page is int page) query["page"] = page.ToString();
        if (options.PageSize is int size) query["pageSize"] = size.ToString();
        if (!string.IsNullOrEmpty(options.Search)) query["search"] = options.Search;
        return query;
    }

    protected async Task<T> GetSingleAsync<T>(RequestDescriptor descriptor, CancellationToken ct) where T : class
    {
        var envelope = await Http.RequestAsync<DataEnvelope<T>>(descriptor, ct).ConfigureAwait(false);
        return envelope?.Data
            ?? throw new InvalidOperationException(
                $"API returned empty or malformed body for {descriptor.Method} {descriptor.Path}");
    }

    protected async Task<PaginatedResponse<T>> GetPagedAsync<T>(RequestDescriptor descriptor, CancellationToken ct)
    {
        var result = await Http.RequestAsync<PaginatedResponse<T>>(descriptor, ct).ConfigureAwait(false);
        return result ?? new PaginatedResponse<T>();
    }

    protected async IAsyncEnumerable<T> PaginateAsync<T>(
        Func<int, RequestDescriptor> buildDescriptor,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var page = 1;
        while (true)
        {
            var descriptor = buildDescriptor(page);
            var response = await GetPagedAsync<T>(descriptor, ct).ConfigureAwait(false);
            foreach (var item in response.Data)
            {
                yield return item;
            }
            if (response.Pagination.TotalPages <= 0 || response.Pagination.Page >= response.Pagination.TotalPages)
            {
                yield break;
            }
            page++;
        }
    }
}
