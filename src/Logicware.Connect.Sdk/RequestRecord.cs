namespace Logicware.Connect.Sdk;

/// <summary>
/// Structured snapshot of one finished HTTP call made by the SDK. Emitted
/// via <see cref="LogicwareConnectOptions.Recorder"/> for the playground /
/// diagnostic flow. <c>X-Api-Key</c> is always masked before reaching here.
/// </summary>
public sealed class RequestRecord
{
    /// <summary>Unique identifier for this record (e.g., <c>req_ab12cd34</c>).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Unix epoch seconds (double) when the request started.</summary>
    public double Timestamp { get; init; }

    public string Method { get; init; } = string.Empty;

    /// <summary>Absolute URL including query string.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary><c>X-Api-Key</c> is replaced with <c>"***"</c>.</summary>
    public IReadOnlyDictionary<string, string> RequestHeaders { get; init; } =
        new Dictionary<string, string>();

    /// <summary>Serialized JSON body, or null for GET / empty bodies.</summary>
    public string? RequestBody { get; init; }

    /// <summary>HTTP status code, or 0 when no response was received.</summary>
    public int Status { get; init; }

    public IReadOnlyDictionary<string, string> ResponseHeaders { get; init; } =
        new Dictionary<string, string>();

    public string ResponseBody { get; init; } = string.Empty;

    public double DurationMs { get; init; }

    /// <summary>Non-null when the request failed before completion
    /// (e.g., <c>"NETWORK: timeout"</c>).</summary>
    public string? Error { get; init; }

    /// <summary>
    /// Original call descriptor so the playground drawer can replay the
    /// request via its proxy endpoint.
    /// </summary>
    public RequestDescriptor Descriptor { get; init; } = new();
}

/// <summary>
/// Shape used internally to invoke the SDK's HTTP layer, and mirrored back
/// on <see cref="RequestRecord.Descriptor"/> for edit-and-resend.
/// </summary>
public sealed class RequestDescriptor
{
    public string Method { get; init; } = "GET";
    public string Path { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string?>? Query { get; init; }
    public object? Body { get; init; }
    public string? IdempotencyKey { get; init; }
}
