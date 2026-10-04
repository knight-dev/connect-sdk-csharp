namespace Logicware.Connect.Sdk.Models;

/// <summary>
/// Standard list envelope returned by every paginated endpoint.
/// </summary>
public sealed class PaginatedResponse<T>
{
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
    public PaginationMeta Pagination { get; init; } = new();
}

public sealed class PaginationMeta
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public class PageOptions
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? Search { get; init; }
}

/// <summary>
/// Wraps a raw-body response (DTO wrapped in a <c>data</c> key). Matches
/// the common <c>{ success, data }</c> envelope the API uses for single-item
/// reads.
/// </summary>
internal sealed class DataEnvelope<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
}
