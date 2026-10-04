using System.Globalization;
using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Models;

namespace Logicware.Connect.Sdk.Resources;

public sealed class RatesResource : ResourceBase
{
    internal RatesResource(LogicwareHttpClient http) : base(http) { }

    /// <summary>
    /// Compute an intake + shipping estimate. Public endpoint — no auth
    /// needed. Useful for showing a quote before a customer commits.
    /// </summary>
    public Task<RateCalculation> CalculateAsync(CalculateRateInput input, CancellationToken cancellationToken = default)
    {
        var q = new Dictionary<string, string?>
        {
            ["warehouseId"] = input.WarehouseId,
            ["weightLbs"] = input.WeightLbs.ToString(CultureInfo.InvariantCulture),
        };
        if (input.LengthInches is decimal l) q["lengthIn"] = l.ToString(CultureInfo.InvariantCulture);
        if (input.WidthInches is decimal w) q["widthIn"] = w.ToString(CultureInfo.InvariantCulture);
        if (input.HeightInches is decimal h) q["heightIn"] = h.ToString(CultureInfo.InvariantCulture);
        if (input.DeclaredValueUsd is decimal v) q["declaredValueUsd"] = v.ToString(CultureInfo.InvariantCulture);
        if (input.FreightType is string f) q["freightType"] = f;
        if (input.DestinationParish is string p) q["destinationParish"] = p;

        return GetSingleAsync<RateCalculation>(
            new RequestDescriptor { Method = "GET", Path = "/api/v1/rates/calculate", Query = q },
            cancellationToken);
    }
}
