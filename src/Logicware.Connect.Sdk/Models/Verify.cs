namespace Logicware.Connect.Sdk.Models;

// Logicware Verify (/api/v1/verify) and Jamaica customs (/api/v1/customs).

public sealed class VerifyItemInput
{
    /// <summary>What the item is, e.g. "Apple AirPods Pro 2".</summary>
    public string ItemDescription { get; set; } = string.Empty;
    public string? MerchantName { get; set; }

    /// <summary>What the shipper declared. Compared with the market value.</summary>
    public decimal? DeclaredValueUsd { get; set; }
    public int? Quantity { get; set; }

    /// <summary>
    /// Your own id for the package. A repeat check of the same reference with the same input
    /// returns the earlier result (<see cref="VerifyResult.Reused"/>, not counted as a scan), and a
    /// receipt is only flagged as "seen before" when it was used for a different reference.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>Re-run even if this reference was already checked with the same input (uses a scan).</summary>
    public bool Force { get; set; }

    /// <summary>
    /// A token you generate (8–64 chars of A–Z, a–z, 0–9, '-', '_'); poll
    /// <c>Verify.ProgressAsync(token)</c> while the request runs. See <c>VerifyResource.NewProgressToken()</c>.
    /// </summary>
    public string? ProgressToken { get; set; }
}

public sealed class VerifyReceiptInput
{
    /// <summary>The receipt file (≤ 10 MB): JPEG, PNG, WebP or PDF.</summary>
    public byte[] File { get; set; } = Array.Empty<byte>();

    /// <summary>"image/jpeg", "image/png", "image/webp" or "application/pdf".</summary>
    public string MimeType { get; set; } = "image/jpeg";

    /// <summary>What the shipper declared. Compared with the receipt total.</summary>
    public decimal? DeclaredValueUsd { get; set; }

    /// <summary>
    /// Your own id for the package. A repeat check of the same reference with the same input
    /// returns the earlier result (<see cref="VerifyResult.Reused"/>, not counted as a scan), and a
    /// receipt is only flagged as "seen before" when it was used for a different reference.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>Re-run even if this reference was already checked with the same input (uses a scan).</summary>
    public bool Force { get; set; }

    /// <summary>
    /// A token you generate (8–64 chars of A–Z, a–z, 0–9, '-', '_'); poll
    /// <c>Verify.ProgressAsync(token)</c> while the request runs. See <c>VerifyResource.NewProgressToken()</c>.
    /// </summary>
    public string? ProgressToken { get; set; }
}

public sealed class VerifyResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    /// <summary>"quota_exceeded", "invalid_file", "file_too_large", "unsupported_merchant", "analysis_failed".</summary>
    public string? ErrorCode { get; init; }
    public string? VerificationId { get; init; }
    public VerificationReport? Report { get; init; }

    /// <summary>The earlier result for the same reference and input — not counted as a scan.</summary>
    public bool Reused { get; init; }

    /// <summary>When the result was produced (the original time when <see cref="Reused"/>).</summary>
    public DateTime? CheckedAtUtc { get; init; }

    /// <summary>Scans used / remaining this month, after this request.</summary>
    public VerifyUsage? Usage { get; init; }
}

/// <summary>Live progress of a running check (see <c>VerifyResource.ProgressAsync</c>).</summary>
public sealed class VerifyProgress
{
    /// <summary>"loading", "reused", "reading", "checking", "pricing", "classifying", "weighing", "customs", "saving" or "done".</summary>
    public string Stage { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool Done { get; init; }
    public List<VerifyProgressStep> Steps { get; init; } = new();
}

public sealed class VerifyProgressStep
{
    public string Stage { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime AtUtc { get; init; }
}

public sealed class VerificationReport
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    /// <summary>"item" or "receipt".</summary>
    public string Mode { get; init; } = "item";

    /// <summary>"verified", "review", "high_risk" or "unable_to_verify".</summary>
    public string Verdict { get; init; } = "unable_to_verify";

    /// <summary>0–100, higher is riskier — the sum of <see cref="Signals"/> points, capped.</summary>
    public int RiskScore { get; init; }
    public double Confidence { get; init; }
    public string? Summary { get; init; }
    public decimal? DeclaredValueUsd { get; init; }
    public decimal? AssessedValueUsd { get; init; }
    public ExtractedReceipt? Receipt { get; init; }
    public List<VerifiedItem> Items { get; init; } = new();
    public List<VerificationSignal> Signals { get; init; } = new();

    /// <summary>Estimated Jamaica customs on the goods, using your customs settings.</summary>
    public CustomsEstimate? Customs { get; init; }

    /// <summary>AI estimate of the main items' packed shipping weight (lbs).</summary>
    public decimal? EstimatedShippingWeightLbs { get; init; }
    public long DurationMs { get; init; }
}

public sealed class VerificationSignal
{
    /// <summary>Stable code, safe to switch on (e.g. "total_mismatch", "declared_below_receipt").</summary>
    public string Code { get; init; } = string.Empty;
    public string Severity { get; init; } = "info";
    public string Source { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Points { get; init; }
}

public sealed class VerifiedItem
{
    public string Description { get; init; } = string.Empty;
    public string? NormalizedName { get; init; }
    public string? Category { get; init; }
    public int Quantity { get; init; }
    public decimal? UnitPricePaidUsd { get; init; }
    public decimal? MarketLowUsd { get; init; }
    public decimal? MarketMedianUsd { get; init; }
    public decimal? MarketHighUsd { get; init; }
    public string PriceSource { get; init; } = "none";
    public List<MarketplaceListing> MatchedListings { get; init; } = new();
    public int ListingsConsidered { get; init; }
    public double DiscountLikelihood { get; init; }
    public string? DiscountReason { get; init; }
    public string Assessment { get; init; } = "unknown";
    public double? PriceRatio { get; init; }
    public string? TariffCode { get; init; }
    public string? TariffName { get; init; }
    public decimal? ImportDutyRate { get; init; }
    public decimal? GctRate { get; init; }
    public decimal? EstimatedWeightLbs { get; init; }

    /// <summary>Packed [length, width, height] in inches.</summary>
    public List<decimal>? EstimatedDimensionsIn { get; init; }
}

public sealed class MarketplaceListing
{
    public string Title { get; init; } = string.Empty;
    public decimal PriceUsd { get; init; }
    public decimal? ListPriceUsd { get; init; }
    public string Marketplace { get; init; } = string.Empty;
    public string? Url { get; init; }
    public string DataSource { get; init; } = string.Empty;
}

public sealed class ExtractedReceipt
{
    public bool IsReceipt { get; init; }
    public string? DocumentType { get; init; }
    public string? Merchant { get; init; }
    public string? OrderNumber { get; init; }
    public string? OrderDate { get; init; }
    public string? Currency { get; init; }
    public decimal? Subtotal { get; init; }
    public decimal? Tax { get; init; }
    public decimal? Shipping { get; init; }
    public decimal? Discount { get; init; }
    public decimal? Total { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Legibility { get; init; }
    public List<ReceiptLine> LineItems { get; init; } = new();
    public List<ForensicFinding> Forensics { get; init; } = new();
}

public sealed class ReceiptLine
{
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? LineTotal { get; init; }
    public decimal? ListPrice { get; init; }
    public string? DiscountNote { get; init; }
}

public sealed class ForensicFinding
{
    public string Code { get; init; } = string.Empty;
    public string Severity { get; init; } = "low";
    public string Description { get; init; } = string.Empty;
}

public sealed class VerifyUsage
{
    public DateTime PeriodStartUtc { get; init; }
    public DateTime PeriodEndUtc { get; init; }

    /// <summary>"free", "bundle" or "payg".</summary>
    public string Plan { get; init; } = "free";
    public string PlanName { get; init; } = string.Empty;
    public int ScansUsed { get; init; }
    public int ScansIncluded { get; init; }
    public int ScansRemaining { get; init; }
    public int ExtraScans { get; init; }
    public bool StopsAtAllowance { get; init; }
    public bool CanScan { get; init; }
    public decimal ChargesSoFarUsd { get; init; }
}

// ── Customs ──────────────────────────────────────────────────────────────

public sealed class CustomsEstimateInput
{
    /// <summary>Value of the goods in USD (all units).</summary>
    public decimal ValueUsd { get; set; }
    public decimal? FreightUsd { get; set; }
    public decimal? InsuranceUsd { get; set; }

    /// <summary>10-digit Jamaica tariff code. Or set <see cref="Description"/> to match one.</summary>
    public string? TariffCode { get; set; }
    public string? Description { get; set; }

    /// <summary>JMD per USD. Defaults to your configured rate.</summary>
    public decimal? ExchangeRate { get; set; }
}

/// <summary>A Jamaica tariff line. Rates are fractions (0.2 = 20%); null = not applicable.</summary>
public sealed class TariffMatch
{
    public string TariffCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    /// <summary>"item" (practical list) or "tariff" (raw tariff line).</summary>
    public string Source { get; init; } = "tariff";
    public string? Group { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public decimal? ImportDuty { get; init; }
    public decimal? Gct { get; init; }
    public decimal? AdditionalStampDuty { get; init; }
    public decimal? SpecialConsumptionTax { get; init; }
    public decimal? Excise { get; init; }
    public decimal? StandardComplianceFee { get; init; }
    public decimal? EnvironmentalLevy { get; init; }
    public bool NeedsManualAssessment { get; init; }
    public string? SpecificRateNote { get; init; }
    public double Score { get; init; }
}

public sealed class CustomsEstimate
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public TariffMatch? Tariff { get; init; }
    public List<TariffMatch> Alternatives { get; init; } = new();
    public decimal ValueUsd { get; init; }
    public decimal CifUsd { get; init; }
    public decimal CifJmd { get; init; }
    public decimal ExchangeRate { get; init; }
    public bool DeMinimisApplied { get; init; }
    public decimal DeMinimisUsd { get; init; }
    /// <summary>What is owed. Empty under de minimis.</summary>
    public List<CustomsChargeLine> Charges { get; init; } = new();

    /// <summary>De minimis only: what the goods would cost above the threshold. Not owed — a preview.</summary>
    public List<CustomsChargeLine> PreviewCharges { get; init; } = new();
    public decimal PreviewTotalJmd { get; init; }
    public List<CustomsGoodsLine> Items { get; init; } = new();
    public decimal TotalJmd { get; init; }
    public decimal TotalUsd { get; init; }
    public decimal EffectiveRate { get; init; }
    public List<string> Notes { get; init; } = new();
    public string TariffVersion { get; init; } = string.Empty;
}

public sealed class CustomsChargeLine
{
    /// <summary>"ID", "ASD", "SCT", "EXC", "SCF", "ENVL", "CAF", "STAMP" or "GCT".</summary>
    public string Code { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Basis { get; init; } = string.Empty;
    public decimal Rate { get; init; }
    public decimal AmountJmd { get; init; }
}

public sealed class CustomsGoodsLine
{
    public string Description { get; init; } = string.Empty;
    public decimal ValueUsd { get; init; }
    public TariffMatch? Tariff { get; init; }
}
