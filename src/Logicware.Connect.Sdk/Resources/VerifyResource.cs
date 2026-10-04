using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

/// <summary>
/// Logicware Verify (beta) — declared-value verification with Jamaica customs estimates.
/// Each successful check is one scan on your Verify plan. Receipt checks currently support
/// Amazon invoices only; other stores return <c>unsupported_merchant</c> (422) and aren't counted.
/// Requires an API key with the <c>verify</c> scope.
/// </summary>
public sealed class VerifyResource : ResourceBase
{
    internal VerifyResource(LogicwareHttpClient http) : base(http) { }

    /// <summary>Check an item's value against live market prices; includes a customs estimate.</summary>
    public Task<VerifyResult> ItemAsync(VerifyItemInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<VerifyResult>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/verify/item", Body = input },
            cancellationToken);

    /// <summary>
    /// Analyse a receipt: extraction, tamper and arithmetic checks, market pricing, tariff
    /// classification, weight estimate and customs estimate.
    /// </summary>
    public Task<VerifyResult> ReceiptAsync(VerifyReceiptInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<VerifyResult>(
            new RequestDescriptor
            {
                Method = "POST",
                Path = "/api/v1/verify/receipt",
                Body = new
                {
                    fileBase64 = Convert.ToBase64String(input.File),
                    mimeType = input.MimeType,
                    declaredValueUsd = input.DeclaredValueUsd,
                    reference = input.Reference,
                    force = input.Force,
                    progressToken = input.ProgressToken,
                },
            },
            cancellationToken);

    /// <summary>Fetch a previous verification.</summary>
    public Task<VerifyResult> GetAsync(string verificationId, CancellationToken cancellationToken = default) =>
        GetSingleAsync<VerifyResult>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/verify/{Escape(verificationId)}" },
            cancellationToken);

    /// <summary>
    /// Live progress of a check started with <c>ProgressToken</c>: current stage and steps so far.
    /// Poll every second or two while <see cref="ItemAsync"/> / <see cref="ReceiptAsync"/> runs.
    /// Null when the token is unknown (not started yet, or expired after 10 minutes).
    /// </summary>
    public async Task<VerifyProgress?> ProgressAsync(string progressToken, CancellationToken cancellationToken = default)
    {
        var envelope = await Http.RequestAsync<DataEnvelope<VerifyProgress>>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/verify/progress/{Escape(progressToken)}" },
            cancellationToken).ConfigureAwait(false);
        return envelope?.Data;
    }

    /// <summary>A random token for <c>ProgressToken</c> / <see cref="ProgressAsync"/>.</summary>
    public static string NewProgressToken() => Guid.NewGuid().ToString("N");

    /// <summary>Scans used this calendar month, the allowance, and charges so far.</summary>
    public Task<VerifyUsage> UsageAsync(CancellationToken cancellationToken = default) =>
        GetSingleAsync<VerifyUsage>(
            new RequestDescriptor { Method = "GET", Path = "/api/v1/verify/usage" },
            cancellationToken);
}

/// <summary>
/// Jamaica customs from the 2026 Integrated Tariff (HS 2022). Deterministic and not metered.
/// Requires an API key with the <c>customs</c> or <c>verify</c> scope.
/// </summary>
public sealed class CustomsResource : ResourceBase
{
    internal CustomsResource(LogicwareHttpClient http) : base(http) { }

    /// <summary>Search tariff lines by item description ("bluetooth speaker", "sneakers").</summary>
    public Task<List<TariffMatch>> SearchTariffsAsync(string query, int limit = 15, CancellationToken cancellationToken = default) =>
        GetSingleAsync<List<TariffMatch>>(
            new RequestDescriptor
            {
                Method = "GET",
                Path = "/api/v1/customs/tariffs/search",
                Query = new Dictionary<string, string?> { ["q"] = query, ["limit"] = limit.ToString() },
            },
            cancellationToken);

    /// <summary>One tariff line with its rates.</summary>
    public Task<TariffMatch> GetTariffAsync(string code, CancellationToken cancellationToken = default) =>
        GetSingleAsync<TariffMatch>(
            new RequestDescriptor { Method = "GET", Path = $"/api/v1/customs/tariffs/{Escape(code)}" },
            cancellationToken);

    /// <summary>Estimate duties, fees and GCT in JMD, by tariff code or description.</summary>
    public Task<CustomsEstimate> EstimateAsync(CustomsEstimateInput input, CancellationToken cancellationToken = default) =>
        GetSingleAsync<CustomsEstimate>(
            new RequestDescriptor { Method = "POST", Path = "/api/v1/customs/estimate", Body = input },
            cancellationToken);
}
