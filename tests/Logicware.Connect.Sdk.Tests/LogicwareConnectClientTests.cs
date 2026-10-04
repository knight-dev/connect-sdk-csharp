using System.Net;
using System.Net.Http;
using Logicware.Connect.Sdk.Exceptions;
using Xunit;

namespace Logicware.Connect.Sdk.Tests;

public sealed class LogicwareConnectClientTests
{
    private static (LogicwareConnectClient, TestHttpMessageHandler) MakeClient(
        Action<LogicwareConnectOptions>? configure = null)
    {
        var handler = new TestHttpMessageHandler();
        var options = new LogicwareConnectOptions
        {
            ApiKey = "sk_test_xyz",
            BaseUrl = new Uri("https://dev-api.logicware.app"),
            HttpClient = new HttpClient(handler),
            MaxAttempts = 3,
        };
        configure?.Invoke(options);
        return (new LogicwareConnectClient(options), handler);
    }

    [Fact]
    public void RejectsEmptyApiKey()
    {
        Assert.Throws<ArgumentException>(() => new LogicwareConnectClient(new LogicwareConnectOptions
        {
            ApiKey = "",
            BaseUrl = new Uri("https://x"),
        }));
    }

    [Fact]
    public void RejectsMissingBaseUrl()
    {
        Assert.Throws<ArgumentException>(() => new LogicwareConnectClient(new LogicwareConnectOptions
        {
            ApiKey = "k",
            BaseUrl = null,
        }));
    }

    [Fact]
    public async Task AttachesApiKeyAndUserAgent()
    {
        var (client, handler) = MakeClient();
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(HttpStatusCode.OK, "{\"data\":[]}"));

        await client.Warehouses.ListAsync();

        var req = Assert.Single(handler.Requests);
        Assert.Equal("sk_test_xyz", req.Headers.GetValues("X-Api-Key").Single());
        // User-Agent is treated as a space-separated list by HttpHeaders —
        // join the tokens back before pattern-matching.
        var ua = string.Join(" ", req.Headers.GetValues("User-Agent"));
        Assert.Matches(@"^Logicware\.Connect\.Sdk/\d+\.\d+\.\d+", ua);
        Assert.Equal("https://dev-api.logicware.app/api/v1/warehouses", req.RequestUri!.ToString());
    }

    [Fact]
    public async Task AppendsUserAgentSuffix()
    {
        var (client, handler) = MakeClient(opts => opts.UserAgentSuffix = "MyCourier/1.0");
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(HttpStatusCode.OK, "{\"data\":[]}"));

        await client.Warehouses.ListAsync();

        var ua = string.Join(" ", handler.Requests.Single().Headers.GetValues("User-Agent"));
        Assert.EndsWith("MyCourier/1.0", ua);
    }

    [Fact]
    public async Task RetriesOn429ThenSucceeds()
    {
        var (client, handler) = MakeClient();
        var tooMany = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        tooMany.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.Zero);
        handler.Responses.Enqueue(tooMany);
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(HttpStatusCode.OK, "{\"data\":{\"id\":\"w_1\",\"code\":\"MIA\",\"name\":\"Miami\",\"freightTypes\":[]}}"));

        var result = await client.Warehouses.GetAsync("w_1");

        Assert.Equal("w_1", result.Id);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task GivesUpAfterMaxAttempts()
    {
        var (client, handler) = MakeClient(opts => opts.MaxAttempts = 2);
        for (int i = 0; i < 2; i++)
        {
            var tooMany = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            tooMany.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.Zero);
            handler.Responses.Enqueue(tooMany);
        }

        var ex = await Assert.ThrowsAsync<LogicwareApiException>(() => client.Warehouses.GetAsync("w_1"));
        Assert.Equal(429, ex.Status);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task MapsValidationError()
    {
        var (client, handler) = MakeClient();
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(
            HttpStatusCode.BadRequest,
            "{\"message\":\"An address code is required on create\",\"code\":\"ADDRESS_CODE_REQUIRED\"}",
            requestId: "req_abc"));

        var ex = await Assert.ThrowsAsync<LogicwareApiException>(() => client.Shippers.CreateAsync(new Models.CreateShipperInput
        {
            Email = "test@example.com",
            Name = "Test",
        }));

        Assert.Equal(400, ex.Status);
        Assert.Equal("ADDRESS_CODE_REQUIRED", ex.ErrorCode);
        Assert.Equal("An address code is required on create", ex.Message);
        Assert.Equal("req_abc", ex.RequestId);
    }

    [Fact]
    public async Task RecorderReceivesMaskedEntry()
    {
        var captured = new List<RequestRecord>();
        var (client, handler) = MakeClient(opts => opts.Recorder = captured.Add);
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(HttpStatusCode.OK, "{\"data\":{\"id\":\"w_1\",\"code\":\"MIA\",\"name\":\"Miami\",\"freightTypes\":[]}}"));

        await client.Warehouses.GetAsync("w_1");

        var record = Assert.Single(captured);
        Assert.Equal("GET", record.Method);
        Assert.Equal(200, record.Status);
        Assert.Equal("***", record.RequestHeaders["X-Api-Key"]);
        Assert.DoesNotContain("sk_test_xyz", System.Text.Json.JsonSerializer.Serialize(record));
        Assert.Equal("/api/v1/warehouses/w_1", record.Descriptor.Path);
    }

    [Fact]
    public async Task RawAsyncReturnsFullPassthrough()
    {
        var (client, handler) = MakeClient();
        handler.Responses.Enqueue(TestHttpMessageHandler.Json((HttpStatusCode)418, "{\"code\":\"teapot\"}"));

        var raw = await client.Http.RawAsync(new RequestDescriptor { Method = "GET", Path = "/api/v1/teapot" });

        Assert.Equal(418, raw.Status);
        Assert.Equal("{\"code\":\"teapot\"}", raw.ResponseBody);
        Assert.Equal("***", raw.RequestHeaders["X-Api-Key"]);
        Assert.Null(raw.Error);
    }
}
