namespace Logicware.Connect.Sdk.Models;

public class CalculateRateInput
{
    public string WarehouseId { get; set; } = string.Empty;
    public decimal WeightLbs { get; set; }
    public decimal? LengthInches { get; set; }
    public decimal? WidthInches { get; set; }
    public decimal? HeightInches { get; set; }
    public decimal? DeclaredValueUsd { get; set; }
    public string? FreightType { get; set; }
    public string? DestinationParish { get; set; }
}

public sealed class RateCalculation
{
    public decimal WeightLbs { get; init; }
    public decimal? BillableWeightLbs { get; init; }
    public decimal BaseIntakeFeeUsd { get; init; }
    public decimal WeightFeeUsd { get; init; }
    public decimal TotalIntakeFeeUsd { get; init; }
    public decimal? ShippingFeeUsd { get; init; }
    public decimal? EstimatedDutyUsd { get; init; }
    public decimal? EstimatedGctJmd { get; init; }
    public decimal? ExchangeRateUsedJmd { get; init; }
    public decimal? TotalEstimatedUsd { get; init; }
}
