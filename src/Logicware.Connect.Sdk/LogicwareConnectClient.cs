using Logicware.Connect.Sdk.Http;
using Logicware.Connect.Sdk.Resources;

namespace Logicware.Connect.Sdk;

/// <summary>
/// Root SDK client for Logicware Connect. Construct once per courier and
/// reuse across the lifetime of your app — each resource property is a
/// lightweight façade over the shared <see cref="LogicwareHttpClient"/>.
/// </summary>
/// <example>
/// <code>
/// using var client = new LogicwareConnectClient(new LogicwareConnectOptions
/// {
///     ApiKey = Environment.GetEnvironmentVariable("LW_API_KEY")!,
///     BaseUrl = new Uri("https://courier-api.logicware.app"),
/// });
///
/// var shipper = await client.Shippers.GetByEmailAsync("customer@example.com");
/// </code>
/// </example>
public sealed class LogicwareConnectClient : IDisposable
{
    public LogicwareHttpClient Http { get; }

    public WarehousesResource Warehouses { get; }
    public ShippersResource Shippers { get; }
    public PackagesResource Packages { get; }
    public ManifestsResource Manifests { get; }
    public PreAlertsResource PreAlerts { get; }
    public IntakeResource Intake { get; }
    public MissingPackagesResource MissingPackages { get; }
    public RatesResource Rates { get; }

    /// <summary>Logicware Verify (beta): value verification with customs estimates. Metered per scan.</summary>
    public VerifyResource Verify { get; }

    /// <summary>Jamaica customs: tariff search and duty estimates (2026 tariff).</summary>
    public CustomsResource Customs { get; }

    public LogicwareConnectClient(LogicwareConnectOptions options)
    {
        Http = new LogicwareHttpClient(options);
        Warehouses = new WarehousesResource(Http);
        Shippers = new ShippersResource(Http);
        Packages = new PackagesResource(Http);
        Manifests = new ManifestsResource(Http);
        PreAlerts = new PreAlertsResource(Http);
        Intake = new IntakeResource(Http);
        MissingPackages = new MissingPackagesResource(Http);
        Rates = new RatesResource(Http);
        Verify = new VerifyResource(Http);
        Customs = new CustomsResource(Http);
    }

    public void Dispose()
    {
        if (Http.OwnsHttpClient)
        {
            Http.Inner.Dispose();
        }
    }
}
