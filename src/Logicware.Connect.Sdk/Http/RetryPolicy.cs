using System.Net;
using System.Net.Http.Headers;

namespace Logicware.Connect.Sdk.Http;

/// <summary>
/// Decides whether a response / network error is retryable and how long
/// to wait. Port of the JS SDK's <c>decideRetry</c>.
/// </summary>
internal static class RetryPolicy
{
    public const int DefaultBaseDelayMs = 500;
    public const int DefaultMaxDelayMs = 8_000;

    // Random.Shared is .NET 6+. A ThreadStatic instance keeps us multi-target
    // without touching a global lock.
    [ThreadStatic] private static Random? _rand;
    private static Random Rand => _rand ??= new Random();

    public static bool IsRetryableStatus(HttpStatusCode status) =>
        status == HttpStatusCode.TooManyRequests
        || status == HttpStatusCode.BadGateway
        || status == HttpStatusCode.ServiceUnavailable
        || status == HttpStatusCode.GatewayTimeout;

    /// <summary>
    /// Picks a wait time for the given attempt. If <paramref name="retryAfter"/>
    /// is set (from the response header), it wins — capped at <paramref name="maxDelayMs"/>.
    /// Otherwise exponential backoff with ±25% jitter.
    /// </summary>
    public static TimeSpan ComputeDelay(
        int attempt,
        RetryConditionHeaderValue? retryAfter,
        int baseDelayMs = DefaultBaseDelayMs,
        int maxDelayMs = DefaultMaxDelayMs)
    {
        if (retryAfter is not null)
        {
            int ms = 0;
            if (retryAfter.Delta is TimeSpan delta)
            {
                ms = (int)Math.Min(delta.TotalMilliseconds, maxDelayMs);
            }
            else if (retryAfter.Date is DateTimeOffset date)
            {
                ms = (int)Math.Max(0, Math.Min((date - DateTimeOffset.UtcNow).TotalMilliseconds, maxDelayMs));
            }
            return TimeSpan.FromMilliseconds(ms);
        }

        var baseDelay = baseDelayMs * Math.Pow(2, attempt - 1);
        var jitterRange = baseDelay * 0.25;
        var jitter = (Rand.NextDouble() * 2 - 1) * jitterRange;
        var wait = Math.Min(Math.Max(0, baseDelay + jitter), maxDelayMs);
        return TimeSpan.FromMilliseconds(wait);
    }

    public static bool IsNetworkRetryable(Exception error)
    {
        var message = error.Message?.ToLowerInvariant() ?? string.Empty;
        return message.Contains("timed out", StringComparison.Ordinal)
            || message.Contains("connection", StringComparison.Ordinal)
            || message.Contains("socket", StringComparison.Ordinal);
    }
}
