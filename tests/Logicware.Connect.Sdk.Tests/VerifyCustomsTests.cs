using System.Net;
using System.Net.Http;
using System.Text.Json;
using Logicware.Connect.Sdk.Models;
using Xunit;

namespace Logicware.Connect.Sdk.Tests;

public sealed class VerifyCustomsTests
{
    private static (LogicwareConnectClient, TestHttpMessageHandler) MakeClient()
    {
        var handler = new TestHttpMessageHandler();
        var client = new LogicwareConnectClient(new LogicwareConnectOptions
        {
            ApiKey = "sk_test",
            BaseUrl = new Uri("https://api.test"),
            HttpClient = new HttpClient(handler),
        });
        return (client, handler);
    }

    [Fact]
    public async Task VerifyItem_posts_and_reads_customs()
    {
        var (client, handler) = MakeClient();
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(HttpStatusCode.OK,
            "{\"success\":true,\"data\":{\"success\":true,\"verificationId\":\"v1\",\"report\":{\"verdict\":\"verified\",\"riskScore\":0,\"customs\":{\"success\":true,\"totalJmd\":9304.37}}}}"));

        var result = await client.Verify.ItemAsync(new VerifyItemInput { ItemDescription = "AirPods Pro 2", DeclaredValueUsd = 199 });

        Assert.Equal("v1", result.VerificationId);
        Assert.Equal(9304.37m, result.Report!.Customs!.TotalJmd);
        Assert.Equal("https://api.test/api/v1/verify/item", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
    }

    [Fact]
    public async Task VerifyReceipt_sends_base64_file()
    {
        var (client, handler) = MakeClient();
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(HttpStatusCode.OK, "{\"data\":{\"success\":true}}"));

        await client.Verify.ReceiptAsync(new VerifyReceiptInput { File = new byte[] { 104, 105 }, MimeType = "image/png", DeclaredValueUsd = 20 });

        using var body = JsonDocument.Parse(handler.Bodies[0]!);
        Assert.Equal("aGk=", body.RootElement.GetProperty("fileBase64").GetString());
        Assert.Equal("image/png", body.RootElement.GetProperty("mimeType").GetString());
    }

    [Fact]
    public async Task Customs_search_passes_query()
    {
        var (client, handler) = MakeClient();
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(HttpStatusCode.OK,
            "{\"data\":[{\"tariffCode\":\"8517130000\",\"name\":\"Smartphone\",\"importDuty\":0.2,\"gct\":0.25}]}"));

        var results = await client.Customs.SearchTariffsAsync("iphone", 5);

        Assert.Equal("8517130000", Assert.Single(results).TariffCode);
        Assert.Equal(0.25m, results[0].Gct);
        Assert.Equal("https://api.test/api/v1/customs/tariffs/search?q=iphone&limit=5", handler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task Rates_send_dimension_keys_the_api_binds()
    {
        var (client, handler) = MakeClient();
        handler.Responses.Enqueue(TestHttpMessageHandler.Json(HttpStatusCode.OK, "{\"data\":{\"weightLbs\":2}}"));

        await client.Rates.CalculateAsync(new CalculateRateInput { WarehouseId = "w1", WeightLbs = 2, LengthInches = 10, WidthInches = 8, HeightInches = 4 });

        var url = handler.Requests[0].RequestUri!.ToString();
        Assert.Contains("lengthIn=10", url);
        Assert.Contains("widthIn=8", url);
        Assert.Contains("heightIn=4", url);
        Assert.DoesNotContain("Inches", url);
    }
}
