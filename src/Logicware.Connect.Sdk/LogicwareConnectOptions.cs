using System.Net.Http;

namespace Logicware.Connect.Sdk;

/// <summary>
/// Construction options for <see cref="LogicwareConnectClient"/>.
/// </summary>
public class LogicwareConnectOptions
{
    /// <summary>
    /// Courier API key. The SDK sends this as <c>X-Api-Key</c> on every request.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Root URL of the courier's API, e.g. <c>https://courier-api.logicware.app</c>.
    /// Resource paths are appended to this — no trailing slash required.
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// Total per-request timeout including retries. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of attempts for one logical request. 1 disables retry.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Appended to the generated <c>User-Agent</c> header, e.g.
    /// <c>"MyCourierApp/1.2.3"</c>. Helps you identify traffic from a specific
    /// integration in the courier's logs.
    /// </summary>
    public string? UserAgentSuffix { get; set; }

    /// <summary>
    /// Optional caller-supplied <see cref="HttpClient"/>. When set, the SDK
    /// uses it verbatim and does NOT apply <see cref="Timeout"/>. Intended for
    /// tests and for wiring into <c>IHttpClientFactory</c>-managed clients.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Optional observer. Invoked once per finished request with a structured
    /// entry (headers masked, full descriptor included) so a demo playground
    /// or diagnostic tool can render the call. Default: null — production
    /// integrations leave this unset and pay no overhead.
    /// </summary>
    public Action<RequestRecord>? Recorder { get; set; }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new ArgumentException("LogicwareConnectOptions.ApiKey is required", nameof(ApiKey));
        }
        if (BaseUrl is null)
        {
            throw new ArgumentException("LogicwareConnectOptions.BaseUrl is required", nameof(BaseUrl));
        }
        if (MaxAttempts < 1)
        {
            throw new ArgumentException("LogicwareConnectOptions.MaxAttempts must be >= 1", nameof(MaxAttempts));
        }
    }
}
