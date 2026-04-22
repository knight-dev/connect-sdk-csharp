# Changelog

All notable changes to `Logicware.Connect.Sdk` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
