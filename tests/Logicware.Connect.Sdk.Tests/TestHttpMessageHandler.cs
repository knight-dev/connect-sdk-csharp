using System.Net;
using System.Net.Http;

namespace Logicware.Connect.Sdk.Tests;

/// <summary>
/// Test-only handler that lets a test inspect every outgoing request and
/// script canned responses. Sequences replay in order; a single handler can
/// also run a delegate for fully custom behaviour.
/// </summary>
public sealed class TestHttpMessageHandler : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = new();
    public Queue<HttpResponseMessage> Responses { get; } = new();

    /// <summary>Request bodies as sent (captured before the content is disposed).</summary>
    public List<string?> Bodies { get; } = new();
    public Func<HttpRequestMessage, HttpResponseMessage>? Handler { get; set; }

    public static HttpResponseMessage Json(HttpStatusCode status, string body, string? requestId = null)
    {
        var response = new HttpResponseMessage(status)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
        };
        if (requestId is not null)
        {
            response.Headers.Add("X-Request-Id", requestId);
        }
        return response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        Bodies.Add(request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult());
        if (Handler is not null)
        {
            return Task.FromResult(Handler(request));
        }
        if (Responses.Count == 0)
        {
            throw new InvalidOperationException("TestHttpMessageHandler received an unexpected request");
        }
        return Task.FromResult(Responses.Dequeue());
    }
}
