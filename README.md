# Logicware.Connect.Sdk

Official .NET SDK for [Logicware Connect](https://logicware.app) — integrate your own courier website with a Logicware-hosted warehouse.

Targets **.NET 8** and **.NET Standard 2.1** (so it works on .NET Framework 4.8+, Unity, and older runtimes via the standard shim).

## Install

```bash
dotnet add package Logicware.Connect.Sdk
```

## Quick start

```csharp
using Logicware.Connect.Sdk;

using var client = new LogicwareConnectClient(new LogicwareConnectOptions
{
    ApiKey = Environment.GetEnvironmentVariable("LW_API_KEY")!,
    BaseUrl = new Uri("https://courier-api.logicware.app"),
});

// Lookup a shipper by email
var shipper = await client.Shippers.GetByEmailAsync("customer@example.com");

// Paginate packages for that shipper
await foreach (var pkg in client.Packages.ListAllAsync(new ListPackagesOptions { ShipperId = shipper.Id }))
{
    Console.WriteLine($"{pkg.TrackingNumber}  {pkg.Status}  {pkg.FreightType}");
}
```

## Dependency injection

In an ASP.NET Core / worker app:

```csharp
using Logicware.Connect.Sdk.DependencyInjection;

builder.Services.AddLogicwareConnect(opts =>
{
    opts.ApiKey = builder.Configuration["Logicware:ApiKey"]!;
    opts.BaseUrl = new Uri(builder.Configuration["Logicware:BaseUrl"]!);
    opts.UserAgentSuffix = "MyCourierPortal/1.0";
});
```

Then inject `LogicwareConnectClient` into controllers / services.

## Resources

Every resource matches the JS and PHP SDKs method-for-method:

| Resource | Examples |
|----------|----------|
| `client.Warehouses` | `ListAsync`, `GetAsync` |
| `client.Shippers` | `ListAsync`, `GetByEmailAsync`, `SyncAsync`, `BulkCreateAsync`, `ImportManyAsync`, `ImportProgressAsync` |
| `client.Shippers.Addresses` | `ListAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` |
| `client.Packages` | `ListAsync`, `ListAllAsync` (async iterator), `GetAsync`, `GetByTrackingAsync`, `ForShipperAsync` |
| `client.Manifests` | `ListAsync`, `CreateAsync`, `SetOpenAsync`, `CloseAsync`, `ReopenAsync`, `FinalizeAsync`, `SetStatusAsync`, `AddPackagesAsync`, `RemovePackageAsync` |
| `client.PreAlerts` | `ListAsync` (returns stats too), `LookupByTrackingAsync`, `CreateAsync`, `CancelAsync` |
| `client.Intake` | `SearchUnidentifiedAsync`, `ListUnclaimedAsync`, `ListReceivedAsync` |
| `client.MissingPackages` | `ListAsync`, `CreateAsync`, `CancelAsync`, `CloseAsync` |
| `client.Rates` | `CalculateAsync` (public — no auth) |

## Errors

- `LogicwareApiException` — non-2xx responses the SDK has stopped retrying on. Carries `Status`, `ErrorCode` (e.g. `"ADDRESS_CODE_REQUIRED"`), `RequestId`, and the parsed `Details`.
- `LogicwareNetworkException` — transport failures (socket, TLS, timeout). Original exception chained as `InnerException`.

## Retries

Built-in: 3 attempts total, exponential backoff with ±25% jitter, honors `Retry-After` on 429/5xx, bails immediately on 4xx (except 429). Override via `LogicwareConnectOptions.MaxAttempts` / `Timeout`.

## Observability

Set `LogicwareConnectOptions.Recorder` to a callable that receives a `RequestRecord` per finished call (with `X-Api-Key` masked). The [demo playground](https://demo-cs.logicware.app) uses this to power its request inspector.

## License

MIT © Logicware
