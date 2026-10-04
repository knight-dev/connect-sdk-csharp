using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Logicware.Connect.Sdk.Exceptions;
using Logicware.Connect.Sdk.Internal;

namespace Logicware.Connect.Sdk.Http;

/// <summary>
/// Thin <see cref="HttpClient"/> wrapper. Attaches auth + user-agent, serializes
/// JSON bodies, retries 429/5xx and transient network errors, and maps non-2xx
/// responses to <see cref="LogicwareApiException"/>. Never leaks raw
/// <see cref="HttpRequestException"/> — everything becomes either
/// <see cref="LogicwareApiException"/> or <see cref="LogicwareNetworkException"/>.
/// </summary>
public sealed class LogicwareHttpClient
{
    public const string SdkName = "Logicware.Connect.Sdk";

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly Uri _baseUrl;
    private readonly int _maxAttempts;
    private readonly TimeSpan _timeout;
    private readonly string _userAgent;
    private readonly Action<RequestRecord>? _recorder;
    private readonly bool _ownsHttpClient;

    public LogicwareHttpClient(LogicwareConnectOptions options)
    {
        options.Validate();

        _apiKey = options.ApiKey;
        _baseUrl = options.BaseUrl!;
        _maxAttempts = options.MaxAttempts;
        _timeout = options.Timeout;
        _recorder = options.Recorder;

        var version = typeof(LogicwareHttpClient).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        _userAgent = string.IsNullOrWhiteSpace(options.UserAgentSuffix)
            ? $"{SdkName}/{version}"
            : $"{SdkName}/{version} {options.UserAgentSuffix}";

        if (options.HttpClient is not null)
        {
            _http = options.HttpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _http = new HttpClient { Timeout = _timeout };
            _ownsHttpClient = true;
        }
    }

    /// <summary>
    /// Send a request and deserialize the JSON response into <typeparamref name="T"/>.
    /// Returns <c>default</c> for 204 / empty bodies.
    /// </summary>
    public async Task<T?> RequestAsync<T>(RequestDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        var (status, body, _) = await SendInternalAsync(descriptor, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(body) || status == 204)
        {
            return default;
        }
        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonConfig.Default);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Send a request and ignore the body (for DELETEs and fire-and-forget POSTs).
    /// </summary>
    public async Task RequestAsync(RequestDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        await SendInternalAsync(descriptor, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Raw pass-through used by the demo playground's proxy endpoint. Captures
    /// the final response verbatim (status, headers, body) without throwing on
    /// non-2xx, so the proxy can mirror every outcome to the browser.
    /// </summary>
    public async Task<RawResponse> RawAsync(RequestDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var url = BuildUrl(descriptor);
        var (maskedHeaders, body) = BuildHeadersAndBody(descriptor);

        HttpResponseMessage? response = null;
        string? error = null;
        try
        {
            using var request = BuildRequest(descriptor, url, body);
            response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            error = $"NETWORK: {ex.Message}";
        }

        var status = response is null ? 0 : (int)response.StatusCode;
        var responseHeaders = response is null ? new Dictionary<string, string>() : FlattenHeaders(response);
        var responseBody = response is null
            ? string.Empty
            : await ReadAsStringCompatAsync(response.Content, cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();
        response?.Dispose();

        RecordEntry(descriptor, url, maskedHeaders, body, status, responseHeaders, responseBody, stopwatch.Elapsed.TotalMilliseconds, error);

        return new RawResponse(status, responseHeaders, responseBody, url, maskedHeaders, body, stopwatch.Elapsed.TotalMilliseconds, error);
    }

    private async Task<(int status, string body, HttpResponseHeaders headers)> SendInternalAsync(
        RequestDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var url = BuildUrl(descriptor);
        var (maskedHeaders, body) = BuildHeadersAndBody(descriptor);

        int attempt = 0;
        while (true)
        {
            attempt++;
            HttpResponseMessage? response = null;
            Exception? networkError = null;

            try
            {
                using var request = BuildRequest(descriptor, url, body);
                response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException tc) when (!cancellationToken.IsCancellationRequested)
            {
                networkError = new LogicwareNetworkException(
                    $"Request timed out after {_timeout.TotalMilliseconds}ms", tc);
            }
            catch (HttpRequestException hre)
            {
                networkError = new LogicwareNetworkException($"Network error: {hre.Message}", hre);
            }

            if (networkError is not null)
            {
                if (attempt < _maxAttempts && RetryPolicy.IsNetworkRetryable(networkError))
                {
                    await Task.Delay(RetryPolicy.ComputeDelay(attempt, retryAfter: null), cancellationToken).ConfigureAwait(false);
                    continue;
                }
                stopwatch.Stop();
                RecordEntry(descriptor, url, maskedHeaders, body, 0, new Dictionary<string, string>(), string.Empty, stopwatch.Elapsed.TotalMilliseconds, networkError.Message);
                throw networkError;
            }

            using (response)
            {
                var status = (int)response!.StatusCode;
                var responseBody = await ReadAsStringCompatAsync(response.Content, cancellationToken).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    stopwatch.Stop();
                    RecordEntry(descriptor, url, maskedHeaders, body, status, FlattenHeaders(response), responseBody, stopwatch.Elapsed.TotalMilliseconds, error: null);
                    return (status, responseBody, response.Headers);
                }

                if (attempt < _maxAttempts && RetryPolicy.IsRetryableStatus(response.StatusCode))
                {
                    var delay = RetryPolicy.ComputeDelay(attempt, response.Headers.RetryAfter);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                stopwatch.Stop();
                var apiException = BuildApiException(response, responseBody);
                RecordEntry(descriptor, url, maskedHeaders, body, status, FlattenHeaders(response), responseBody, stopwatch.Elapsed.TotalMilliseconds, error: null);
                throw apiException;
            }
        }
    }

    private HttpRequestMessage BuildRequest(RequestDescriptor descriptor, string url, string? body)
    {
        var request = new HttpRequestMessage(new HttpMethod(descriptor.Method), url);
        request.Headers.TryAddWithoutValidation("Accept", "application/json");
        request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
        request.Headers.TryAddWithoutValidation("X-Api-Key", _apiKey);
        if (!string.IsNullOrWhiteSpace(descriptor.IdempotencyKey))
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", descriptor.IdempotencyKey);
        }
        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }
        return request;
    }

    private (IReadOnlyDictionary<string, string> maskedHeaders, string? body) BuildHeadersAndBody(RequestDescriptor descriptor)
    {
        // These are for recorder / proxy output only — the real request
        // headers get rebuilt on every retry attempt inside BuildRequest.
        var masked = new Dictionary<string, string>
        {
            ["Accept"] = "application/json",
            ["User-Agent"] = _userAgent,
            ["X-Api-Key"] = "***",
        };
        if (!string.IsNullOrWhiteSpace(descriptor.IdempotencyKey))
        {
            masked["Idempotency-Key"] = descriptor.IdempotencyKey!;
        }

        string? body = null;
        if (descriptor.Body is not null)
        {
            body = JsonSerializer.Serialize(descriptor.Body, JsonConfig.Default);
            masked["Content-Type"] = "application/json";
        }
        return (masked, body);
    }

    private string BuildUrl(RequestDescriptor descriptor)
    {
        var trimmedBase = _baseUrl.ToString().TrimEnd('/');
        var path = descriptor.Path.StartsWith("/", StringComparison.Ordinal) ? descriptor.Path : "/" + descriptor.Path;
        var url = trimmedBase + path;
        if (descriptor.Query is null || descriptor.Query.Count == 0)
        {
            return url;
        }

        var builder = new StringBuilder(url);
        var sep = '?';
        foreach (var kv in descriptor.Query)
        {
            if (kv.Value is null) continue;
            builder.Append(sep).Append(Uri.EscapeDataString(kv.Key)).Append('=').Append(Uri.EscapeDataString(kv.Value));
            sep = '&';
        }
        return builder.ToString();
    }

    private static IReadOnlyDictionary<string, string> FlattenHeaders(HttpResponseMessage response)
    {
        var flat = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            flat[header.Key] = string.Join(", ", header.Value);
        }
        foreach (var header in response.Content.Headers)
        {
            flat[header.Key] = string.Join(", ", header.Value);
        }
        return flat;
    }

    private static LogicwareApiException BuildApiException(HttpResponseMessage response, string body)
    {
        var status = (int)response.StatusCode;
        response.Headers.TryGetValues("X-Request-Id", out var requestIdValues);
        var requestId = requestIdValues?.FirstOrDefault();

        var message = $"HTTP {status}";
        string? code = null;
        object? details = body;

        if (!string.IsNullOrEmpty(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                details = JsonSerializer.Deserialize<Dictionary<string, object?>>(body, JsonConfig.Default);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                    {
                        message = msg.GetString() ?? message;
                    }
                    else if (doc.RootElement.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
                    {
                        message = err.GetString() ?? message;
                    }
                    if (doc.RootElement.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == JsonValueKind.String)
                    {
                        code = codeProp.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                var snippet = body.Length > 200 ? body.Substring(0, 200) : body;
                message = $"HTTP {status}: {snippet}";
            }
        }

        return new LogicwareApiException(message, status, code, requestId, details);
    }

    private void RecordEntry(
        RequestDescriptor descriptor,
        string url,
        IReadOnlyDictionary<string, string> requestHeaders,
        string? requestBody,
        int status,
        IReadOnlyDictionary<string, string> responseHeaders,
        string responseBody,
        double durationMs,
        string? error)
    {
        if (_recorder is null) return;
        try
        {
            var record = new RequestRecord
            {
                Id = "req_" + Guid.NewGuid().ToString("N").Substring(0, 12),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d,
                Method = descriptor.Method,
                Url = url,
                RequestHeaders = requestHeaders,
                RequestBody = requestBody,
                Status = status,
                ResponseHeaders = responseHeaders,
                ResponseBody = responseBody,
                DurationMs = durationMs,
                Error = error,
                Descriptor = descriptor,
            };
            _recorder(record);
        }
        catch
        {
            // Recorder must never break the caller.
        }
    }

    /// <summary>
    /// Result of a raw pass-through. All fields are safe to serialize — the
    /// masked request headers are suitable for a browser-facing proxy payload.
    /// </summary>
    public sealed record RawResponse(
        int Status,
        IReadOnlyDictionary<string, string> ResponseHeaders,
        string ResponseBody,
        string RequestUrl,
        IReadOnlyDictionary<string, string> RequestHeaders,
        string? RequestBody,
        double DurationMs,
        string? Error);

    internal bool OwnsHttpClient => _ownsHttpClient;
    internal HttpClient Inner => _http;

    /// <summary>
    /// Shim for <c>HttpContent.ReadAsStringAsync(CancellationToken)</c>
    /// which only exists on .NET 5+. On netstandard2.1 we fall back to the
    /// zero-arg version with a best-effort cancellation via Task.WhenAny.
    /// </summary>
    private static async Task<string> ReadAsStringCompatAsync(HttpContent content, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1
        var readTask = content.ReadAsStringAsync();
        if (!cancellationToken.CanBeCanceled)
        {
            return await readTask.ConfigureAwait(false);
        }
        var cancelTcs = new TaskCompletionSource<bool>();
        using var reg = cancellationToken.Register(() => cancelTcs.TrySetResult(true));
        var completed = await Task.WhenAny(readTask, cancelTcs.Task).ConfigureAwait(false);
        if (completed == cancelTcs.Task)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
        return await readTask.ConfigureAwait(false);
#else
        return await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
    }
}
