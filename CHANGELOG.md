# Changelog

All notable changes to `Logicware.Connect.Sdk` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0] — 2026-10-03

### Added
- `client.Verify` — Logicware Verify (beta): `ItemAsync`, `ReceiptAsync` (raw bytes), `GetAsync`, `UsageAsync`. Reports include per-item tariff classification, packed weight/dimension estimates and a Jamaica customs estimate. Needs the `verify` scope.
- `client.Customs` — Jamaica customs from the 2026 tariff: `SearchTariffsAsync`, `GetTariffAsync`, `EstimateAsync`. Needs the `customs` or `verify` scope.

### Fixed
- `Rates.CalculateAsync` sent `lengthInches`/`widthInches`/`heightInches`, which the API doesn't bind, so dimensions were silently ignored. It now sends `lengthIn`/`widthIn`/`heightIn`.

## [0.2.0] — 2026-04-24

### Added
- `Package` now exposes `PackageType`, `Condition`, `ConditionNotes`, `SourceMarketplace`, `MerchantName`, plus full dimensions: `LengthIn`, `WidthIn`, `HeightIn`, `DimensionalWeightLbs`, `BillableWeightLbs`. Strings are returned as-is from the V1 API enum names (Box / Bag / Envelope / Tube / Crate / Pallet / Irregular / Other for `PackageType`; Good / MinorDamage / ModerateDamage / SevereDamage / Tampered / WetDamaged / Fragile for `Condition`).
- `CreatePackageInput` accepts `PackageType`, `SourceMarketplace`, `MerchantName` (in addition to the existing `FreightType`).
- `UpdatePackageInput` accepts `LengthIn`, `WidthIn`, `HeightIn`, `PackageType`, `Condition`, `ConditionNotes`, `SourceMarketplace`, `MerchantName` (in addition to the existing `FreightType` and `Status`).

## [0.1.1] — 2026-04-21

First .NET release. Full parity with `@logicware.app/connect-sdk` (0.1.1) and `logicware/connect-sdk` (PHP, 0.2.0).

### Added

**Core**
- `LogicwareConnectClient` with `LogicwareConnectOptions` (ApiKey, BaseUrl, Timeout, MaxAttempts, UserAgentSuffix, HttpClient, Recorder).
- `LogicwareHttpClient` PSR-style transport:
  - `X-Api-Key` auth header
  - `User-Agent: Logicware.Connect.Sdk/<version> <suffix?>`
  - JSON (System.Text.Json, camelCase)
  - 3-attempt exponential backoff with ±25% jitter, honors `Retry-After` on 429/5xx
  - `Idempotency-Key` / `X-Request-Id` pass-through via `RequestDescriptor`
  - Retries transient network errors (timeout, connection reset) as well as 429/502/503/504

**Resources**
- `Warehouses` — `ListAsync`, `GetAsync`.
- `Shippers` — `ListAsync`, `ListAllAsync` (async iterator), `GetAsync`, `GetByEmailAsync`, `GetByCodeAsync`, `CreateAsync`, `UpdateAsync`, `SyncAsync` (upsert-by-email), `BulkCreateAsync`, `ImportManyAsync`, `GetImportAsync`, `GetImportFailuresAsync`, `ImportProgressAsync` (async iterator).
- `Shippers.Addresses` — CRUD for secondary addresses.
- `Packages` — `ListAsync`, `ListAllAsync`, `GetAsync`, `GetByTrackingAsync`, `CreateAsync`, `UpdateAsync`, `ForShipperAsync`, `ForManifestAsync`.
- `Manifests` — `ListAsync`, `GetAsync`, `CreateAsync`, `UpdateAsync`, `SetOpenAsync`, `CloseAsync`, `ReopenAsync`, `FinalizeAsync`, `SetStatusAsync`, `AddPackagesAsync`, `RemovePackageAsync`, `DeleteAsync`.
- `PreAlerts` — `ListAsync` (returns `PreAlertStats` alongside data), `ListAllAsync`, `GetAsync`, `LookupByTrackingAsync`, `CreateAsync`, `CancelAsync`.
- `Intake` — `SearchUnidentifiedAsync`, `ListUnclaimedAsync`, `ListReceivedAsync`, and their async-iterator variants.
- `MissingPackages` — `ListAsync`, `ListAllAsync`, `GetAsync`, `CreateAsync`, `CancelAsync`, `CloseAsync`.
- `Rates.CalculateAsync` (public — no auth required).

**Errors**
- `LogicwareException` (base), `LogicwareApiException` (status, errorCode, requestId, details), `LogicwareNetworkException` (wraps transport failures).

**Observability**
- `LogicwareConnectOptions.Recorder` — optional `Action<RequestRecord>` invoked per finished request with a structured snapshot (headers masked, full descriptor for replay). Opt-in; zero overhead when null.

**Dependency injection**
- `services.AddLogicwareConnect(configure)` extension registers the client as a singleton wired through `IHttpClientFactory`.

**Targets**: `net8.0` and `netstandard2.1` (works on .NET Framework 4.8, Unity, older runtimes via the standard shim).

[Unreleased]: https://github.com/knight-dev/connect-sdk-csharp/compare/v0.1.1...HEAD
[0.1.1]: https://github.com/knight-dev/connect-sdk-csharp/releases/tag/v0.1.1
