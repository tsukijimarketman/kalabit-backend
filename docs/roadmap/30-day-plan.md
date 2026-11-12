# Kalabit: 30-day plan to a public v0.1

Written 12 Nov 2026. It covers Fri 13 Nov to Sat 12 Dec 2026. Weekdays get 1 hour and weekends get 5, which comes to about 66 hours.

This plan doesn't replace the PRD or the Build Plan. Both still describe the destination. This plan is the free-tier path to a first public release, and it changes the order of the work: deploy early, measure, then scale.

> Rule for every day: read for at most 15 minutes, then type. End the day with a commit and a push, using a Conventional Commit message. If something takes longer than planned, move it to the next day rather than skipping its "done when" check.

---

## 1. Where the codebase is today

| Area | State | Action |
|---|---|---|
| .NET 10 pin, central package management, nullable reference types, warnings as errors | Done | Small upgrade-proofing cleanup (Day 1) |
| Entities, configurations, `InitialCreate` migration | Done; check constraints and `xmin` row versions already in place | Revise for v0.1 (Day 3) |
| State machine and its tests | Done | Add `Queued → Sending` (Day 4) |
| Fluid renderer with step and output caps, and its tests | Done | Reuse at accept time and in the worker |
| Compose with Postgres and Mailpit | Done | Pin the Mailpit tag |
| Health endpoints (controller) | Done | Move to built-in health checks (Day 8) |
| `SendNotificationRequest` and the accepted-response contract | Started | Finish (Day 11) |
| Migrator | **Only checks the connection.** It never calls `MigrateAsync` | Fix (Day 3) |
| `NotificationDbContextFactory` | Reads only `appsettings*.json` | Add environment variables so it works on the server (Day 3) |
| CI | **Missing** (`.github/` doesn't exist) | Day 2 |
| Leftovers | `WeatherForecast*`, three `Class1.cs` files | Delete (Day 1) |
| Worker, Web API, load-test worker | Empty folders | Days 18, 24 and 28 |

You are roughly at Build Plan Week 3, Day 1.

---

## 2. Set everything up now, or grow step by step?

**Grow step by step, with guard rails up front.** Split the work into two kinds:

| Kind | Examples | When |
|---|---|---|
| **Guard rails**: cheap now, compound forever | CI, central packages, a single target framework, structured logging, health checks, migrations as a job, secrets out of git, a deploy script, tests | **Set up early** (Days 1–9) |
| **Capacity infrastructure**: expensive, and only pays off under load you don't have yet | Kafka, Keycloak, Redis/Valkey, Prometheus/Jaeger, Kubernetes, multiple instances | **Defer** until a measurement tells you to add it |

Two rules make deferring safe.

1. **Design for replacement, not for scale.** Put an interface (a *seam*) wherever you know a bigger component will arrive later, so swapping it changes one class and not the whole codebase:
   - Queue: today a Postgres table claimed with `FOR UPDATE SKIP LOCKED`; later outbox → Kafka.
   - `IEmailSender`: today MailKit; later per-client SMTP profiles, or provider APIs.
   - Authentication: today API keys through a custom authentication handler; later OAuth client credentials added as a *second scheme*.
   - Rate limiting: today in-memory; later Valkey when there is more than one API instance.
2. **Scale on evidence.** Each scaling step in section 9 has a *trigger metric*. You don't add Kafka because the PRD says so. You add it when the queue-age metric says Postgres polling is the bottleneck. That is also a much stronger story to tell in an interview.

**Deploy a walking skeleton on Day 9.** Deploying is where free tiers, IIS and Windows hurt. Finding that out in week 2 is cheap, and finding it out in week 4 is a crisis.

---

## 3. Free-tier architecture for v0.1 (and the decisions that differ from the PRD)

```
 Client apps ──HTTPS──► IIS site "kalabit-api"  (NotificationSenderApi)
                             │  API key auth, validate, idempotency, quota
                             ▼
                  Supabase Postgres (free, schema "kalabit")
                             ▲
                             │  claim due rows with FOR UPDATE SKIP LOCKED
 Windows Service "Kalabit Worker" (NotificationWorker) ──SMTP──► Brevo / Gmail (prod)
                                                                 Mailpit (dev/load tests)
 Operators ─HTTPS─► IIS site "kalabit-app"  (NotificationWebApi + React build in wwwroot)
 Task Scheduler ─daily─► supabase-keepalive.ps1 ─psql─► heartbeat row
 Optional: Cloudflare Tunnel exposes both IIS sites publicly with no open router ports
```

Write each decision below as an ADR in `docs/adr/` (Day 2). Each one names its revisit trigger.

| # | PRD target | v0.1 choice | Why | Revisit when |
|---|---|---|---|---|
| ADR-001 | .NET 10 LTS | Same | Supported to Nov 2028; skip .NET 11 (STS) and move to .NET 12 LTS in late 2027 | .NET 12 GA |
| ADR-002 | Kafka + outbox relay | **Postgres as the queue**: the notification row is the job, claimed with `FOR UPDATE SKIP LOCKED` and a lease. The `outbox_messages` table stays in the schema but nothing writes to it yet | No free way to run Kafka reliably next to IIS; Postgres queues comfortably handle hundreds of messages a second | Claim queries dominate DB CPU, or you need a second consumer type |
| ADR-003 | Keycloak client credentials | **API keys** (`Authorization: Bearer kb_…`), the model Stripe, Resend, Postmark and SendGrid use | Keycloak is heavy to host; API keys are an industry standard for server-to-server email APIs | A customer asks for OAuth; add it as a second scheme |
| ADR-004 | Keycloak with a BFF for the dashboard | **ASP.NET Core Identity** with a cookie (HttpOnly, Secure, SameSite=Strict) and roles | Standard, free, in-process; no tokens in the browser | Single sign-on is needed |
| ADR-005 | Containers and Compose in prod | **IIS** for the two APIs and the dashboard; **Windows Service** for the worker | IIS recycles app pools and isn't a host for long-running background loops; Windows Services are | You move to containers or a VPS |
| ADR-006 | Self-hosted Postgres | **Supabase free tier** through its *session pooler* (IPv4) | Free managed Postgres | You hit the 500 MB limit or need point-in-time recovery |
| ADR-007 | Route `POST /api/v1.1/Email/SendEmail` | **`POST /api/v1/emails`** and **`GET /api/v1/emails/{id}`** | No legacy callers exist, so use resource-style routes (your own 12 Oct note) | Never; version only on breaking changes |
| ADR-008 | JSON Schema check at accept time | **Trial render** with Fluid strict variables at accept time | Zero new dependencies; catches missing variables with a 422 | You need type checks (numbers vs strings) |
| ADR-009 | Swagger/Scalar off in production | **Scalar and the OpenAPI document ON for the Sender API in production** | The API contract *is* the product's documentation (Resend and Postmark publish theirs) | Never for Sender; keep it off for the admin Web API |

### Supabase specifics you must handle

- **Use the session pooler connection string** (`aws-0-<region>.pooler.supabase.com:5432`, username `postgres.<project-ref>`). On the free tier the direct connection is IPv6-only, and most home ISPs and Windows setups need IPv4. Session mode supports everything you use: transactions, `SKIP LOCKED` and advisory locks. Avoid transaction mode (port 6543) for now.
- Add `SSL Mode=Require` and `Maximum Pool Size=10` per process (three processes → 30 connections, well under the free limit).
- **Put your tables in a `kalabit` schema, not `public`.** Supabase exposes `public` through its auto-generated REST API (PostgREST). Also turn the Data API off, or keep `kalabit` out of its exposed schemas, in the project settings.
- **Two database roles:** the `postgres` owner role is used only by the Migrator; a `kalabit_app` login role with SELECT, INSERT, UPDATE and DELETE on schema `kalabit` is used by the APIs and the worker (least privilege).
- **500 MB is your real capacity limit.** Treat retention jobs (Day 23) as a must-have, not a nice-to-have. **Never load-test against Supabase.** Load tests run against local Docker Postgres.
- Free projects pause after about a week without database activity. The worker polls every few seconds, so it keeps the project awake. The keepalive script is the safety net for when the worker is stopped.

### Free SMTP for real delivery (check current limits when you sign up)

- **Dev, CI and load tests:** Mailpit, always.
- **Production demo:** Brevo free tier (around 300 emails a day, needs sender verification) or Gmail SMTP with an app password (needs 2FA; low daily cap; the From address must be your Gmail). With your own domain you can set SPF, DKIM and DMARC, which the PRD requires. A domain is the one cost worth paying for (about US$10 a year).

### Exposing the IIS box publicly for free

- **Cloudflare Tunnel** (`cloudflared` as a Windows service): no ports open on your router, free TLS. A stable hostname needs a domain on Cloudflare; without one, a "quick tunnel" gives a random URL that changes on every restart.
- Alternative: DuckDNS (free subdomain) plus win-acme (free Let's Encrypt certificates for IIS) plus port 443 forwarded on your router. This exposes your home IP.
- In both cases, add the **forwarded headers middleware** (trusting the `X-Forwarded-Proto` and `X-Forwarded-For` headers that `cloudflared` adds). Otherwise `UseHttpsRedirection` can loop forever behind the tunnel.

---

## 4. What to install

### On the dev and IIS machine

| Tool | Why | Notes |
|---|---|---|
| .NET 10 SDK | You have it | `global.json` already pins it |
| IIS (Windows Features: Web Server, plus **Application Initialization**) | Hosts the APIs and the dashboard | Turn it on before the hosting bundle |
| **.NET 10 Hosting Bundle** | ASP.NET Core Module V2 for IIS | Install *after* IIS, then run `iisreset` |
| Docker Desktop | You have it | Local Postgres, Mailpit, Testcontainers |
| Node LTS (through your nvm) | React dashboard | Day 30 |
| **k6** | Load testing | `winget install k6`; AGPL, so run it unmodified as a separate program |
| **PostgreSQL command-line tools** (psql, pg_dump) | Keepalive script, backups | The EDB installer lets you pick "Command Line Tools" only |
| `gh` CLI | Repository, releases, PRs | Optional |
| `cloudflared` | Public exposure | Optional (Day 23) |

### dotnet local tools (add to `dotnet-tools.json`)

- `dotnet-outdated-tool`: shows which packages are behind and is your upgrade radar. You already have `dotnet-ef` and `reportgenerator`.

### NuGet packages (versions go in `Directory.Packages.props`, never in a csproj)

| Project | Package | Purpose |
|---|---|---|
| NotificationSenderApi, NotificationWebApi | `Serilog.AspNetCore`, `Serilog.Sinks.File` | Structured JSON logs (console and rolling file for IIS) |
| NotificationSenderApi, NotificationWebApi | `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | Standard `/health/ready` |
| NotificationSenderApi | `Microsoft.Extensions.ApiDescription.Server` (private assets) | Writes the OpenAPI document at build time so CI can diff it |
| NotificationWorker | `MailKit` | SMTP |
| NotificationWorker | `Microsoft.Extensions.Hosting.WindowsServices` | Run as a Windows Service |
| NotificationWorker | `Serilog.Extensions.Hosting`, `Serilog.Settings.Configuration`, `Serilog.Sinks.Console`, `Serilog.Sinks.File` | Logging without ASP.NET |
| NotificationWebApi | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Operator accounts and roles |
| UnitTests | `Shouldly` (or `AwesomeAssertions`), `Microsoft.Extensions.TimeProvider.Testing` | Readable asserts; `FakeTimeProvider` |
| IntegrationTests | `Testcontainers.PostgreSql`, `Microsoft.AspNetCore.Mvc.Testing` | A real Postgres and an in-memory API host |
| IntegrationTests | `NetArchTest.Rules` (optional) | Enforce the dependency rules |

Excluded on licence grounds: MediatR, AutoMapper, MassTransit 9 and FluentAssertions 8. Before adding anything, check its licence on nuget.org; allow only MIT, Apache-2.0, BSD and PostgreSQL licences.

Frontend (Day 30): the Vite `react-ts` template, `@tanstack/react-query`, `react-router`, and `openapi-typescript` plus `openapi-fetch` for a client generated from your OpenAPI document.

---

## 5. Entity and data-model changes for v0.1 (Day 3)

Nothing is deployed yet, so you may **delete `InitialCreate` and regenerate it** once. After the first Supabase deployment (Day 9), migrations are forward-only, forever. Write that rule into ADR-006.

| Entity or table | Change | Lesson |
|---|---|---|
| `NotificationDbContext` | `HasDefaultSchema("kalabit")` | Schemas, Supabase exposure |
| `Entity` | Remove the `DateTimeOffset.UtcNow` default; set `CreatedAt` (and `UpdatedAt` on `AuditableEntity`) in a **`SaveChangesInterceptor`** that uses the injected **`TimeProvider`** | Interceptors, testable time (`FakeTimeProvider`) |
| `Client` | Add `Name` (required, unique, max 100). Make `KeycloakClientId` nullable with its unique index filtered to non-null values (kept for future OAuth). Add an `ApiKeys` navigation | Filtered unique indexes |
| **New `ApiKey`** | `Id`, `ClientId` (foreign key), `Name`, `Prefix` (unique, max 16, the lookup key), `KeyHash` (bytea, 32 bytes, SHA-256), `Scopes` (`string[]`, which Npgsql maps to `text[]` natively), `ExpiresAt?`, `RevokedAt?`, `LastUsedAt?` | Never store secrets, only hashes. API keys are high-entropy, so a plain SHA-256 is correct; bcrypt is for passwords |
| `Template` | Add `ClientId` (owner). Replace the unique `TemplateKey` index with a unique index on (`ClientId`, `TemplateKey`) | Tenant isolation |
| `TemplateVersion` | Unique (`TemplateId`, `VersionNumber`); add `PublishedAt?` | Immutability |
| `NotificationIdentity` | `IdempotencyKey` becomes **non-null** (the header is required); keep the unique (client, key) index | Idempotency |
| `Notification` | Add `AttemptCount`, `NextAttemptAt` (initially set to creation time), `LeaseOwner?`, `LeaseExpiresAt?`, `LastFailureClass?`, `LastError?` (max 500, sanitized), `FinalizedAt?`. Change `required byte[]? EncryptedPayloadEnvelope` to plain `byte[]?` (`required` plus nullable is contradictory, and retention will null it). Add **partial index** `ix_notifications_due` on `next_attempt_at` where status is `Queued` or `Retrying`, and `ix_notifications_expired_leases` on `lease_expires_at` where status is `Sending`. Add a check constraint `attempt_count >= 0` | Partial indexes, queue design |
| `NotificationRecipient` | Apply the same `required byte[]?` fix to `EncryptedAddressEnvelope` | |
| `NotificationAttempt` | Add `FailureClass?` and `ErrorMessage?` (sanitized, max 500) | |
| `OutboxMessage` | Keep it, but nothing writes to it (ADR-002) | Planned evolution |
| **New `ClientDailyUsage`** | Composite key (`ClientId`, `UsageDate` as a `DateOnly` → `date`), `Count`. Updated with an atomic upsert (`INSERT … ON CONFLICT … DO UPDATE … RETURNING`) | Race-free counters |
| **New `KeepAliveHeartbeat`** | `Id` (bigint identity), `PingedAt`, `Source` | |
| Lib enums | Add `FailureClass` (Transient, Permanent, Poison) | |
| Later (Day 24) | A separate `AdminIdentityDbContext` holding the Identity tables in the same `kalabit` schema, with its own migrations folder | Multiple contexts; keeps Identity out of the Sender API |

Update `docs/data-model.md` (the Mermaid diagram) in the same commit.

---

## 6. Milestones

| Milestone | Days | Demonstrable result | Social post |
|---|---|---|---|
| **M0: Foundations** | 1–3 | Clean build, green CI, v0.1 schema, a Migrator that really migrates | #1 "Building Kalabit in public" (architecture sketch) |
| **M1: Walking skeleton in prod** | 4–9 | `/health/ready` green on IIS against Supabase; keepalive scheduled | #2 "Deployed on day 9: IIS + Supabase for $0" |
| **M2: Accept path** | 10–17 | Authenticated, idempotent, rate-limited `POST /api/v1/emails` live, documented in Scalar | #3 idempotency explainer with a diagram |
| **M3: Delivery pipeline** | 18–23 | Real email from production; retries, dead letters, crash recovery, retention | #4 a 30-second clip from curl to inbox |
| **M4: Operate and measure** | 24–29 | Admin API, Identity roles, k6 baseline in `docs/performance.md` | #5 "How many emails per second can a $0 stack sustain?" |
| **M5: v0.1 launch** | 30 | Dashboard live, README polished, tag `v0.1.0` | #6 launch post + dev.to article |

---

## 7. Day by day

Legend: **Learn** = concepts to read about (15 minutes at most). **Do** = what you type. **Done when** = the check you must pass before committing.

### Week 1: Foundations and the walking skeleton

**Day 1 · Fri 13 Nov · 1h · Clean-up and upgrade-proofing**
- Learn: how MSBuild imports `Directory.Build.props`; why pinning `LangVersion` hurts upgrades.
- Do:
  - Delete `WeatherForecast.cs`, `WeatherForecastController.cs` and every `Class1.cs`.
  - Remove the `<PropertyGroup>` that repeats `TargetFramework`, `Nullable` and `ImplicitUsings` from each csproj; those values come from `Directory.Build.props`.
  - Remove `<LangVersion>14.0</LangVersion>`, because the language version should follow the target framework.
  - Pin `axllent/mailpit` to a version tag instead of `latest`.
  - Add `.env.example` with placeholder values.
  - Add `dotnet-outdated-tool` to the tool manifest.
- Done when: `dotnet build` succeeds with zero warnings, and a search for `net10.0` matches only `Directory.Build.props`.

**Day 2 · Sat 14 Nov · 5h · CI, ADRs, Supabase project**
- Learn: GitHub Actions jobs and steps; Dependabot grouping; ADR format (context, decision, consequences).
- Do:
  - `.github/workflows/ci.yml`: checkout, then `actions/setup-dotnet` with `global-json-file`, restore with the locked tool manifest, `dotnet build`, `dotnet test` with coverage, and upload the coverage report as an artifact.
  - `.github/dependabot.yml`: weekly updates for `nuget` and `github-actions`, with Microsoft and EF packages grouped.
  - Write ADR-001 through ADR-009 from section 3, one short file each.
  - Create the Supabase project in the region nearest you (Singapore if you're in the Philippines). Save the session-pooler connection string in a password manager, never in git.
  - Make the repository public (free CI minutes and CodeQL) and add the CI badge to the README.
- Done when: CI is green on a pushed commit, and the badge renders.
- Post #1.

**Day 3 · Sun 15 Nov · 5h · v0.1 schema and a real Migrator**
- Learn: `IDesignTimeDbContextFactory`; `Database.MigrateAsync` and `GetPendingMigrationsAsync`; partial indexes; `SaveChangesInterceptor`.
- Do:
  - Make every entity change in section 5 (except the Identity context).
  - Add `.AddEnvironmentVariables()` to `NotificationDbContextFactory` so `ConnectionStrings__NotificationDatabase` works on the server.
  - The Migrator logs the pending migrations, applies them, logs what it applied, and returns exit code 0 on success and 1 on failure.
  - Delete and regenerate `InitialCreate`, then read the SQL from `dotnet ef migrations script --idempotent`, table by table.
- Done when: running the Migrator against fresh local Docker Postgres creates the `kalabit` schema; running it twice changes nothing; the ER diagram is updated.

**Day 4 · Mon 16 Nov · 1h · State machine for the Postgres queue**
- Do: allow `Queued → Sending`. `Published` is reserved for the Kafka phase, so add an XML doc comment saying so. Add a `IsFinal(status)` helper. Add `[Theory]` rows for the new transition and for every final state.
- Done when: the tests are green and you can explain why `Published` is skipped in v0.1.

**Day 5 · Tue 17 Nov · 1h · Retry policy (pure function)**
- Learn: exponential backoff with full jitter (AWS Architecture Blog).
- Do: in `Domain/Retry`, write a `RetryPolicy` that takes the attempt number, a base delay, a maximum delay and a random source, and returns the next delay. Pass the random source in so tests can be deterministic.
- Done when: table-driven tests prove the delay stays in [0, min(cap, base·2^n)] and that a fixed seed gives a fixed result.

**Day 6 · Wed 18 Nov · 1h · Failure classifier (pure function)**
- Do: add a `FailureClass` enum in Lib. In `Domain/Retry`, write a classifier that takes *primitives only*: an optional SMTP status code and a Domain enum such as `ConnectionRefused`, `Timeout`, `AuthFailed` or `Protocol`. Domain must not reference MailKit. Rules: 2xx is not a failure, 4xx is transient, 5xx is permanent, network errors are transient, and anything unknown is transient (the attempt cap stops it).
- Done when: tests cover 421, 450, 550, 554, refused, timeout and unknown.

**Day 7 · Thu 19 Nov · 1h · API key generator and verifier**
- Learn: `RandomNumberGenerator`, `SHA256.HashData`, `CryptographicOperations.FixedTimeEquals`, and why comparisons must take constant time.
- Do: in Domain (it's pure, using only the BCL), the key format is `kb_` + a 12-character prefix + `_` + a 32-byte base64url secret. Write three functions: generate (returns the plaintext once, plus the prefix and the hash), parse (rejects malformed keys), and verify.
- Done when: tests show keys are unique, malformed input is rejected, a tampered key fails, and the plaintext never appears in any stored model.

**Day 8 · Fri 20 Nov · 1h · Logging and standard health checks**
- Do:
  - Add Serilog to the Sender API: `UseSerilog` reading its settings from configuration, a compact JSON console sink, a daily rolling file sink (IIS has no console), and `UseSerilogRequestLogging`.
  - Replace `HealthController` with `AddHealthChecks().AddDbContextCheck<…>(tags: ready)`. Map `/health/live` with no checks and `/health/ready` with the ready tag. Your controller was the learning step; this is the industry-standard version.
- Done when: behaviour matches before, and stopping Postgres turns `/health/ready` into 503.

**Day 9 · Sat 21 Nov · 5h · Walking skeleton in production** *(the most important day of the month)*
- Learn: IIS app pools; in-process hosting; `web.config` generated by publish; `app_offline.htm`; Task Scheduler.
- Do:
  1. Turn on IIS with Application Initialization, then install the .NET 10 Hosting Bundle, then run `iisreset`.
  2. In Supabase's SQL editor, create the `kalabit_app` login role and grant it usage on schema `kalabit` plus default privileges on its tables. Turn off Data API exposure for `kalabit`.
  3. Run the Migrator from your machine against Supabase, as the owner role.
  4. Create the IIS site `kalabit-api` with its own app pool: **No Managed Code**, Start Mode `AlwaysRunning`, Idle Time-out `0`, and Preload Enabled on the site. Set the app pool's environment variables: `ASPNETCORE_ENVIRONMENT=Production` and `ConnectionStrings__NotificationDatabase` (session pooler, `kalabit_app`, SSL required, pool size 10).
  5. Write `scripts/deploy/Deploy-SenderApi.ps1`: run `dotnet publish -c Release` to a staging folder, drop `app_offline.htm` into the site folder, mirror the files over with robocopy, remove `app_offline.htm`, then call `/health/ready` and fail the script if it isn't 200.
  6. Write `scripts/ops/Invoke-SupabaseKeepAlive.ps1`. Set `$ErrorActionPreference = 'Stop'`, and take the password from `%APPDATA%\postgresql\pgpass.conf`, never from the script. Call `psql` in single-transaction mode with `ON_ERROR_STOP=1`, and in that one transaction insert a heartbeat row and delete heartbeat rows older than 30 days. Append a timestamped result line to a log file, and exit non-zero on failure.
  7. Register a scheduled task that runs the script daily, runs whether or not you are logged in, and has **"run as soon as possible after a missed start"** turned on (your PC may be off at the scheduled time).
- Done when: `http://localhost:<port>/health/ready` returns Healthy from IIS against Supabase; the task's first run inserted a heartbeat row; redeploying with the script works without file-lock errors.
- Write `docs/runbooks/deploy.md` and `docs/runbooks/supabase-keepalive.md`. Post #2.

### Week 2: The accept path

**Day 10 · Sun 22 Nov · 5h · API key authentication**
- Learn: the ASP.NET Core authentication pipeline; `AuthenticationHandler<TOptions>`; claims and `ClaimsPrincipal`; authorization policies; the difference between 401 and 403.
- Do:
  - `ApiKeyAuthenticationHandler` in `NotificationSenderApi/Authentication`. It reads `Authorization: Bearer kb_…`, looks the key up by prefix (the unique index), verifies the hash, and rejects revoked or expired keys and inactive clients. It then builds claims for the client ID, client name and each scope.
  - Cache lookups in `IMemoryCache` for 60 seconds. Update `LastUsedAt` at most once every 5 minutes, not on every request.
  - Add the policy `notifications:send`, which requires that scope.
  - Add a Development-only Migrator verb, `seed-demo-client`, that creates a client and prints its API key once. This is temporary until the admin API on Day 24.
  - Write the cases in `NotificationSenderApi.http`.
- Done when, checked by hand: no key → 401 (with a `WWW-Authenticate` header); revoked key → 401; missing scope → 403; inactive client → 403.

**Day 11 · Mon 23 Nov · 1h · Contract and problem details**
- Do:
  - Finalise `SendEmailRequest`: `To` (1–50 recipients), `TemplateKey`, `Data` (`JsonElement`), `ExpiresAt?`. Use DataAnnotations plus `IValidatableObject` for rules across fields.
  - Validate recipients with `MailAddress.TryCreate` and reject any CR or LF.
  - Add the `EmailStatusResponse` record.
  - Wire up `AddProblemDetails` (with customisation that adds `correlationId` and `traceId`), `UseExceptionHandler` and `UseStatusCodePages`.
  - Set the request size limit to 256 KB.
- Done when: a bad body returns a 400 RFC 9457 problem document with an `errors` dictionary, and an oversized body returns 413.

**Day 12 · Tue 24 Nov · 1h · Correlation ID middleware**
- Do: middleware that reads `X-Correlation-Id` or creates a UUIDv7, stores it in `HttpContext.Items`, pushes it into Serilog's `LogContext`, and echoes it in the response header.
- Done when: every log line from a request carries `CorrelationId`, and so does the problem document.

**Day 13 · Wed 25 Nov · 1h · Payload protection and recipient hashing**
- Learn: AES-GCM (nonce, tag, associated data); HMAC with a pepper.
- Do:
  - In `Data/Security`, write `AesGcmPayloadProtector`. Envelope layout: format byte, key ID, 12-byte nonce, 16-byte tag, ciphertext. Keys come from configuration as `Kalabit:Encryption:Keys:<id>` (32 bytes, base64) plus `ActiveKeyId`.
  - Write `RecipientHasher` (HMAC-SHA256 with a pepper from configuration).
  - Bind both with the options pattern and `ValidateOnStart`.
  - Keep keys in user secrets locally and in app pool or service environment variables on the server.
- Done when: tests cover the round trip, a flipped byte raising `CryptographicException`, an unknown key ID, and decrypting with an old key after the active key rotates.

**Day 14 · Thu 26 Nov · 1h · Template lookup and trial render**
- Do:
  - Write a recursive converter from `JsonElement` to a dictionary (a good exercise in pattern matching on `JsonValueKind`).
  - Write `TemplateResolver`: find (client, key), active, latest *published* version.
  - Cache parsed templates by version ID. Published versions are immutable, which is exactly why caching them is safe.
  - Trial-render the subject, HTML and text with strict variables. Map a missing variable to a 422 that names it.
- Done when: unit tests cover a found template, a missing one (404) and a missing variable (422).

**Day 15 · Fri 27 Nov · 1h · Masking and the handler skeleton**
- Do: `EmailMasker` (`juan@example.com` becomes `j***@example.com`), with tests for edge cases such as single-character local parts. Create a plain `SendEmailHandler` class (no mediator) with its steps as private methods, still empty bodies.
- Done when: the masker tests are green and the handler compiles.

**Day 16 · Sat 28 Nov · 5h · Accept path complete, with integration tests**
- Learn: one `SaveChangesAsync` call is already one transaction; unique-violation SQLSTATE 23505; what a 202 promises.
- Do:
  - In `SendEmailHandler`:
    1. Require the `Idempotency-Key` header (400 if missing).
    2. Compute the payload hash: SHA-256 of the request serialised the same way every time.
    3. Look up an existing identity. Same hash → return the original response with `Idempotent-Replayed: true`. Different hash → 409.
    4. Insert the `NotificationIdentity`, the `Notification` (encrypted payload; `NextAttemptAt` set to now) and the recipients (hash, masked address, encrypted address) in **one** `SaveChangesAsync`.
    5. If two requests race and the insert hits 23505, reload the winner and return a replay.
    6. Return 202 with `Location: /api/v1/emails/{id}`.
  - `GET /api/v1/emails/{id}` returns 404 for notifications owned by another client.
  - Create the `IntegrationTests` project: Testcontainers Postgres, the Migrator's migrations applied in a fixture, and `WebApplicationFactory`. Tests: happy path, replay, conflict, **the same key sent 20 times in parallel creating exactly one row**, cross-client 404.
  - Deploy with your script.
- Done when: CI runs the integration tests green and production accepts a real request.
- Post #3.

**Day 17 · Sun 29 Nov · 5h · Rate limits, quota, an excellent contract**
- Do:
  - `AddRateLimiter` with a `PartitionedRateLimiter` token bucket keyed by client ID. Read the limits from the client row through the cache. `OnRejected` returns 429, `Retry-After` and a problem document. The in-memory state resets when the app pool recycles; that's acceptable for one instance and is noted in ADR-005.
  - Daily quota: an atomic upsert on `ClientDailyUsage` in the same transaction as the insert, returning 429 when it's exhausted. Use UTC dates.
  - OpenAPI:
    - Add `ProducesResponseType` for 202, 400, 401, 403, 404, 409, 413, 422, 429 and 503.
    - Add request examples.
    - Add a document transformer for the Bearer security scheme so Scalar can send your key.
    - Turn on build-time generation and commit the generated document to `docs/openapi/sender-v1.json`.
  - Turn Scalar on in production for the Sender API (ADR-009).
- Done when: a burst test sees 429s with a correct `Retry-After`; changing the limit in the database takes effect within the cache TTL; Scalar on IIS can send a valid request. **M2 is complete.**

### Week 3: The delivery pipeline

**Day 18 · Mon 30 Nov · 1h · Worker skeleton**
- Do:
  - Run `dotnet new worker` into `src/NotificationWorker` and add it to the solution with references to Data, Domain, Lib and Templating.
  - Add Serilog and `AddWindowsService`, and set `HostOptions.ShutdownTimeout` to 30 seconds.
  - Add `WorkerOptions` (batch size, parallelism, lease duration, polling backoff) and `SmtpOptions` (host, port, security mode, user, password, from address), both validated on start.
  - Register `AddDbContextFactory`.
- Done when: it starts, logs, and logs a clean shutdown on Ctrl+C.

**Day 19 · Tue 1 Dec · 1h · The claim query**
- Learn: `FOR UPDATE SKIP LOCKED`; `UPDATE … RETURNING`; leases.
- Do: `NotificationClaimer` runs one raw-SQL `UPDATE` (through `Database.SqlQuery` or `FromSql`, always parameterised). The `UPDATE` sets status `Sending`, the lease owner, the lease expiry and `attempt_count + 1` on rows chosen by a sub-select. The sub-select picks rows that are due (`Queued` or `Retrying` with `next_attempt_at <= now()`) **or** abandoned (`Sending` with an expired lease), ordered by `next_attempt_at`, with a `LIMIT` of the batch size, locked with `FOR UPDATE SKIP LOCKED`. It returns the claimed IDs.
- Done when: an integration test runs two claimers at once and shows no overlap and no row lost.

**Day 20 · Wed 2 Dec · 1h · MailKit sender**
- Do:
  - Put `IEmailSender` in the worker (or Lib/Common if the Web API's test send will need it) and implement `MailKitEmailSender`.
  - Build the `MimeMessage` with `BodyBuilder` (HTML plus text gives multipart/alternative). Set `MessageId` to `<{notificationId}@{yourdomain}>`.
  - Use `SecureSocketOptions` from configuration: `None` only for Mailpit, `StartTls` in production. Set the timeouts.
  - Map MailKit exceptions (`SmtpCommandException.StatusCode`, `SocketException`, timeouts) to the Domain classifier's inputs.
- Done when: a test sends to Mailpit and the Message-ID matches.

**Day 21 · Thu 3 Dec · 1h · The processing loop**
- Do: a `BackgroundService` that loops on claim, then processes the batch with `Parallel.ForEachAsync`, using a maximum degree of parallelism from configuration and a fresh `DbContext` for each item. For each item:
  1. Load it.
  2. If it has expired, mark it `Expired`.
  3. Decrypt the payload and recipients.
  4. Render the template.
  5. Send.
  6. Record the outcome as a **compare-and-set** with `ExecuteUpdateAsync`: the `WHERE` clause requires status `Sending` *and* the lease owner to be this worker.

  When a batch comes back empty, wait 1 second, doubling to at most 5.
- Done when: a POST to the API makes an email appear in local Mailpit within a few seconds.

**Day 22 · Fri 4 Dec · 1h · Failures and retries**
- Do: on failure, classify it, write a `NotificationAttempt` every time, then decide:
  - Transient with attempts left → `Retrying`, with `NextAttemptAt` = now + `RetryPolicy`.
  - Permanent → `Failed`.
  - Attempts exhausted, or a poison message (cannot decrypt, unknown template) → `DeadLettered`.

  Store sanitized error text only: no addresses, no message bodies.
- Done when: with Mailpit stopped and short delays set in configuration, a notification climbs the attempts and is sent once Mailpit restarts.

**Day 23 · Sat 5 Dec · 5h · Hardening and real email in production**
- Do:
  - **Tests:** a Mailpit container in Testcontainers (assert through its HTTP API), a fake `IEmailSender` for failure paths, a crash-recovery test (a short lease, abandon the worker mid-send, and a second worker reclaims the message), and a graceful-shutdown test.
  - **Retention service:** a daily background service that nulls the payload and recipient envelopes 7 days after `FinalizedAt` and deletes heartbeat rows older than 30 days. Use `TimeProvider` so tests can control the clock.
  - **Windows Service install:** publish the worker and register it with `New-Service`. Set recovery to restart on failure (`sc.exe failure`). Put SMTP credentials, the encryption keys and the connection string in machine-level environment variables, or in the service's registry environment. Write `scripts/deploy/Deploy-Worker.ps1`: stop the service, copy the files, start it, check the logs.
  - **Real SMTP:** configure Brevo or Gmail.
  - Optional: Cloudflare Tunnel, plus the forwarded headers middleware.
- Done when: a curl against production delivers a real email to your inbox; all the tests are green in CI. **M3 is complete.**
- Post #4 (a screen recording from curl to inbox).

### Week 4: Operate, measure, launch

**Day 24 · Sun 6 Dec · 5h · Admin Web API with Identity**
- Learn: ASP.NET Core Identity; cookie authentication; anti-forgery tokens; role-based policies.
- Do:
  - Create `src/NotificationWebApi` with controllers, OpenAPI, Serilog and health checks.
  - Add `AdminIdentityDbContext` with its own migrations folder, and make the Migrator apply both contexts.
  - Add a Migrator verb, `seed-admin`, that reads the email and password from environment variables.
  - Configure the cookie: HttpOnly, Secure, SameSite=Strict, sliding expiry, and lockout on.
  - Add the roles Viewer, Operator and Admin as policies.
  - Endpoints: `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`; client endpoints (list, create, edit limits, deactivate); API key endpoints (create, which shows the key **once**; list, which shows prefixes only; revoke).
  - Remove the temporary `seed-demo-client` verb.
- Done when: log in through Scalar, create a client and a key, and that key sends through the Sender API.

**Day 25 · Mon 7 Dec · 1h · Templates admin**
- Do: create a template, add a draft version, edit the draft (optimistic concurrency through `xmin`, returning 409 on a stale edit), publish, and deprecate.
- Done when: a test proves a published version cannot be edited.

**Day 26 · Tue 8 Dec · 1h · Preview and notification search**
- Do:
  - A preview endpoint that renders a template version with sample data and returns the subject, HTML, text and any variable errors.
  - `GET /api/admin/notifications`: keyset paging on (`created_at`, `id`), filters for status, client and date range, and masked recipients.
  - A detail endpoint that includes the attempt timeline.
- Done when: `EXPLAIN` on the search query shows index use on 100k rows seeded *locally*.

**Day 27 · Wed 9 Dec · 1h · Overview statistics**
- Do: `GET /api/admin/stats?range=24h` returns counts by status and the accept-to-SMTP latency at p50, p95 and p99 (`percentile_cont` in raw SQL through `SqlQuery`). Add an index if `EXPLAIN` asks for one.
- Done when: a hand-built dataset gives exactly the expected numbers in a test.

**Day 28 · Thu 10 Dec · 1h · k6 scripts**
- Learn: open versus closed workload models; arrival-rate executors; thresholds.
- Do:
  - `load-tests/scenarios/smoke.js`: unique idempotency keys; the URL and key come from environment variables.
  - `load-tests/scenarios/accept-rate.js`: a `ramping-arrival-rate` staircase, with thresholds of error rate below 1%, p95 below 200 ms, and no dropped iterations.
  - Target a **local release build against Docker Postgres and Mailpit**, never Supabase.
- Done when: the smoke test passes 100% of its checks.

**Day 29 · Fri 11 Dec · 1h · Baseline and performance report**
- Do: run the staircase and save the k6 summary JSON to `load-tests/results/`. Write `docs/performance.md`: hardware, versions, the method, the highest step that passed, and what saturated first (watch Task Manager and `docker stats`). Also measure processing rate (worker throughput into Mailpit).
- Done when: the document names the sustainable accept rate, the processing rate and the bottleneck.

**Day 30 · Sat 12 Dec · 5h · Dashboard v0.1 and launch**
- Do:
  - In `kalabit-frontend`, create a Vite React TypeScript app and generate its types from the Web API's OpenAPI document.
  - Pages: Login, Overview (status tiles plus the latency figures), Notifications (list and detail with the timeline), and Clients and API keys (create, with the key shown once, and revoke).
  - Serving: the Web API serves the built app from `wwwroot` with `UseStaticFiles` plus `MapFallbackToFile("index.html")`. That keeps one IIS site and one origin, so there's no CORS and the SameSite=Strict cookie works.
  - The deploy script builds the frontend and copies `dist/` into the Web API's publish folder.
  - Create the IIS site `kalabit-app` with its own app pool.
  - Polish the README: a one-paragraph pitch, the architecture diagram, a GIF, the quickstart, links to the ADRs and `docs/performance.md`, and a LICENSE (MIT or Apache-2.0).
  - Tag `v0.1.0` and create a GitHub release with notes.
- Done when: a stranger can read the README, get an API key from you, and send an email.
- Post #6, plus a dev.to or Hashnode article: *"A $0 transactional email platform: Postgres SKIP LOCKED instead of Kafka, and when I'd switch."*

> If you slip: Days 25–27 and the dashboard are the flexible part. Never cut Day 9 (deploy), Day 16 (idempotency tests) or Day 23 (crash recovery and retention). Those are what make it production-grade and not just a demo.

---

## 8. Production checklist (IIS + Windows Service + Supabase)

- [ ] Each app has its own app pool: No Managed Code, `AlwaysRunning`, idle time-out 0, preload on.
- [ ] Secrets live only in app pool or service environment variables, never in `appsettings.Production.json` and never in git. Run gitleaks in CI.
- [ ] `ASPNETCORE_ENVIRONMENT=Production`; developer exception page off; HSTS on; forwarded headers on if behind a tunnel.
- [ ] Serilog writes rolling files to a folder outside the site folder, with a retention count set.
- [ ] Health checks: `/health/live` and `/health/ready` on both APIs. Point the free UptimeRobot plan at the `/health/ready` URLs; it's free uptime monitoring and also keeps Supabase active.
- [ ] Worker service: automatic (delayed start), restart on failure, and running as a dedicated low-privilege local account.
- [ ] Supabase: `kalabit_app` least-privilege role, Data API not exposing `kalabit`, session pooler, SSL required.
- [ ] Backups: a weekly `pg_dump` scheduled task to a local folder, kept for 4 weeks. The free tier's own backups are limited.
- [ ] Keepalive task scheduled, with its log checked weekly.
- [ ] A runbook for each: deploy, rollback (redeploy the previous release folder), rotate an API key, rotate the encryption key, Supabase paused, worker stuck.

---

## 9. What to scale, in order, after Day 30

Move to the next step only when its **trigger** fires. Each step should be its own ADR, a before-and-after k6 run, and a post.

| # | Step | Trigger (measure it first) | Why this order |
|---|---|---|---|
| 1 | **Finish the product surface**: template editor and preview UI, a suppression list with RFC 8058 one-click unsubscribe, an audit log | Before any scaling. Missing features block users; missing scale doesn't yet | Product value first |
| 2 | **Observability**: OpenTelemetry traces and metrics (`notif_*` from the PRD), exported to a free Grafana Cloud tier or to Prometheus and Jaeger in Docker | You can't answer "why is p95 slow?" from logs alone | You can't scale what you can't see |
| 3 | **Tune the single node**: worker parallelism, SMTP connection reuse (one connected client per parallel slot, instead of a connection per email), batch claim size, an EF compiled query on the hot path | Queue age is growing while CPU is low (I/O-bound on SMTP) | Free capacity, no new infrastructure |
| 4 | **Resilience**: a Polly circuit breaker around SMTP (through `Microsoft.Extensions.Resilience`) that pauses claiming while it is open | The SMTP provider has outages that burn retry attempts | Protects the retry budget |
| 5 | **Per-client sender profiles** with envelope-encrypted SMTP secrets | A second real client wants its own From domain | The real multi-tenant product |
| 6 | **Status webhooks** to clients (signed with HMAC, retried) | Clients are polling `GET /emails/{id}` too often | What Resend and Postmark do |
| 7 | **Data growth**: monthly partitioning of `notifications` (your composite (`id`, `created_at`) key is already ready for it); drop old partitions instead of deleting rows | Over ~1M rows, retention deletes are slow, or you're approaching 500 MB, at which point upgrading the DB is also an option | Keeps writes and purges cheap |
| 8 | **More worker processes** (same box, or a second machine) | One worker is CPU-bound or at its parallelism limit | `SKIP LOCKED` already makes this safe, so it's free |
| 9 | **More API instances and a shared rate limiter** (Valkey, BSD) | One API process can't hold the accept-rate target | Rate limiting must be shared once there are 2+ instances |
| 10 | **Outbox and a broker** (Kafka, Redpanda or RabbitMQ) behind the queue seam; turn on `Published` and the relay | Claim queries dominate DB load, or you need several consumer types (webhooks, analytics) | Most expensive to run, so it comes last |
| 11 | **OAuth client credentials** (Keycloak) as a second auth scheme | An enterprise client requires it | API keys keep working |
| 12 | **Containers and orchestration** (Docker images, then Kubernetes with KEDA scaling on queue lag) | You leave the single box | The PRD's stretch goals |
| 13 | **LoadTestWorker**: the server runs k6 from the dashboard, with caps, an allow-list and audit, following the PRD's safety controls | After step 2 (it needs backend metrics) | The full benchmark product |

---

## 10. Keeping it upgrade-friendly (.NET 11, 12 and beyond)

- **One place to change the framework:** the target framework lives only in `Directory.Build.props`, and the SDK only in `global.json` (`rollForward: latestFeature`). An upgrade then means two edits, a build and the tests.
- **Follow LTS:** stay on .NET 10 until **.NET 12 LTS (Nov 2027)**. Try .NET 11 in a branch for learning, but don't ship on short-term-support releases.
- **Central packages + Dependabot + `dotnet outdated`** every month. Microsoft packages move together, so upgrade them as a group.
- **Warnings as errors** make obsolete APIs fail the build early. During a framework upgrade, temporarily demote the specific `SYSLIB`/`CS0618` IDs with `WarningsNotAsErrors`, fix them, then remove the exception.
- **Keep Domain free of frameworks** (pure C#, BCL only). It never breaks on upgrades, and it's your most-tested code.
- **Tests are your upgrade safety net:** integration tests on real Postgres catch EF and Npgsql behaviour changes. Check the EF Core "breaking changes" page for each release.
- **IIS:** install the *new* hosting bundle **before** deploying an app built for the new framework (bundles install side by side), then run `iisreset`.
- **Testing stack:** plan a move from xUnit v2 to **xUnit v3** as a small, separate PR.
- **Forward-only migrations** (expand, then contract) let you roll back by redeploying the previous release folder without touching the DB.

---

## 11. Building in public

- **Cadence:** one post per milestone (6 in 30 days), on LinkedIn and X. Each post carries **one proof artifact**: a diagram, a 20–30 second clip, a test output, or a k6 chart. Proof beats claims.
- **Format:** the problem, then what you built, then the one concept you learned (for example idempotency or `SKIP LOCKED`), then a link to the commit or ADR.
- **Long-form:** two articles on dev.to or Hashnode after launch: (1) idempotent APIs with Postgres unique constraints; (2) a Postgres queue vs Kafka, with your k6 numbers.
- **Your learning notes** (`docs/notes/`) are content. Each weekly note can become a post with almost no extra work.
- **Never post** connection strings, API keys or Supabase URLs with keys. Blur screenshots, and keep gitleaks in CI.
- **Repo hygiene for visitors:** README badges (CI, coverage), ADRs linked from the README, `docs/performance.md`, a LICENSE, `SECURITY.md`, and tagged releases.

---

## 12. Gotchas to expect

| Symptom | Cause | Fix |
|---|---|---|
| HTTP 500.19 or 500.31 on IIS | Hosting bundle missing, or installed before IIS | Reinstall the bundle, then run `iisreset` |
| Files locked during deploy | The app is still running | Use `app_offline.htm` in the deploy script |
| Worker stops after about 20 minutes on IIS | A background loop hosted in IIS | That's why the worker is a Windows Service |
| Cannot connect to Supabase from Windows | The direct host is IPv6-only | Use the session pooler string |
| Too many connections | Each process has a pool of 100 by default | `Maximum Pool Size=10` |
| Infinite redirect behind the tunnel | HTTPS redirect plus a plain-HTTP origin | Forwarded headers middleware |
| Rate limits reset at odd times | App pool recycling clears in-memory state | Expected for v0.1 (ADR-005); scaling step 9 |
| Supabase project paused | No DB activity for about a week | Keepalive task, UptimeRobot on `/health/ready`, a running worker |
| Database close to 500 MB | No retention, or load tests hit prod | Day 23 retention; load-test locally only |
