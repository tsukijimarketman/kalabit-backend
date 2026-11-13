# ADR-0001: Target .NET 10 LTS and upgrade only from LTS to LTS

- **Status:** Accepted
- **Date:** 2026-11-13
- **PRD reference:** Tech stack (p.28–31); Build Plan p.1–2

## Context

Kalabit is a long-lived product maintained by one person. Each framework upgrade takes time away from features: packages need to move together, there may be breaking changes, and CI and the servers have to follow. .NET alternates between Long-Term Support (LTS) releases, supported for three years, and Standard-Term Support (STS) releases. .NET 10 is the current LTS (supported until November 2028). .NET 11 (STS) shipped in November 2026, and .NET 12 (the next LTS) is expected in November 2027.

## Decision

We target **.NET 10 LTS** and skip .NET 11. We move to **.NET 12 LTS** after it ships, once its EF Core and Npgsql versions are stable.

The upgrade is kept to as few files as possible:

- `global.json` pins the SDK feature band with `rollForward: latestFeature`. Patch and feature updates are picked up, but a major version change has to be deliberate.
- `Directory.Build.props` is the only place that sets `TargetFramework`, `Nullable`, `ImplicitUsings` and `TreatWarningsAsErrors`.
- `Directory.Packages.props` (central package management) is the only place that sets package versions.
- No `LangVersion` pin; the C# version follows the target framework.
- CI reads the SDK version from `global.json` (`actions/setup-dotnet` with `global-json-file`).

## Consequences

**Positive**

- One planned upgrade every two years instead of one every year.
- The upgrade is a short, reviewable diff: `global.json`, `Directory.Build.props` and the package versions.
- Local builds, CI and the servers always use the same SDK rules.

**Negative**

- New .NET 11 runtime and C# features are not available until .NET 12.
- Dependabot would happily propose .NET 11 packages, which do not run on .NET 10. To stop that, `.github/dependabot.yml` ignores **major** updates for packages that version with .NET (`Microsoft.AspNetCore.*`, `Microsoft.EntityFrameworkCore*`, `Microsoft.Extensions.*`, `Npgsql*`). Patch and minor updates still arrive weekly.
- That ignore rule has to be removed on purpose when we upgrade, as part of the .NET 12 work.

## Revisit when

- .NET 12 reaches general availability (expected November 2027). Plan the upgrade within six months, well before .NET 10 support ends in November 2028.
- A security fix or a feature we need exists only in a newer major version.
