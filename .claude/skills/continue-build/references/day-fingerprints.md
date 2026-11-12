# Day fingerprints: repo evidence that a day is done

Check them with Glob and Grep. A day counts as done only when **all** of its fingerprints are present **and** its "Done when" check from `docs/roadmap/30-day-plan.md` holds (verify by building or running tests when you can). Names are the plan's suggestions; if the user named something differently, search by intent (for example grep `SKIP LOCKED` instead of a class name) and accept equivalent work.

| Day | Fingerprints (all required) |
|---|---|
| 1 | No `**/Class1.cs`; no `WeatherForecast*`; `LangVersion` absent from `Directory.Build.props`; `TargetFramework` appears in no `src/**/*.csproj`; `.env.example` exists; `dotnet-outdated-tool` in `dotnet-tools.json`; the Mailpit image in `docker-compose.yml` is not `:latest` |
| 2 | `.github/workflows/*.yml` runs `dotnet test`; `.github/dependabot.yml`; at least 9 ADR files in `docs/adr/` (not counting `.gitkeep`); CI badge in `README.md` |
| 3 | `HasDefaultSchema("kalabit")`; `ApiKey`, `ClientDailyUsage` and `KeepAliveHeartbeat` entities; a class deriving from `SaveChangesInterceptor`; `TimeProvider` used for `CreatedAt`; Migrator calls `MigrateAsync`; `AddEnvironmentVariables` in `NotificationDbContextFactory`; `Notification` has `NextAttemptAt` and `LeaseExpiresAt`; `docs/data-model.md` mentions `api_keys` |
| 4 | The `Queued` arm of `NotificationStateMachine` allows `Sending`; an `IsFinal` method; tests reference both |
| 5 | A retry policy in `src/Domain/Retry/` that takes a random source; unit tests for it |
| 6 | `FailureClass` enum in `src/Lib/Enums/`; a classifier in `src/Domain/Retry/` with no MailKit reference; table-driven tests |
| 7 | API key generation and verification in Domain using `RandomNumberGenerator` and `FixedTimeEquals`; tests |
| 8 | `Serilog.AspNetCore` referenced by the Sender API; `AddHealthChecks` with a DbContext check; `/health/ready` mapped; `HealthController.cs` removed |
| 9 | `scripts/deploy/Deploy-SenderApi.ps1`; `scripts/ops/Invoke-SupabaseKeepAlive.ps1`; `docs/runbooks/deploy.md`; `docs/runbooks/supabase-keepalive.md`. You can't verify IIS or Supabase from the repo, so **ask the user** to confirm `/health/ready` on IIS and the heartbeat row |
| 10 | A class deriving from `AuthenticationHandler<`; a `notifications:send` policy; a `seed-demo-client` verb in the Migrator |
| 11 | `SendEmailRequest` (or the renamed contract) with validation; `AddProblemDetails`; a request size limit |
| 12 | Correlation ID middleware pushing to `LogContext`; the `X-Correlation-Id` header |
| 13 | `AesGcm` used in Data/Security; HMAC recipient hasher; options with `ValidateOnStart`; tests for tamper and rotation |
| 14 | A `JsonElement` → dictionary converter; a template resolver; a parsed-template cache; a 422 mapping for missing variables |
| 15 | `EmailMasker` plus tests; a `SendEmailHandler` class |
| 16 | The `api/v1/emails` route; `Idempotency-Key` handling with `Idempotent-Replayed`; 23505 handling; a `tests/NotificationPlatform.IntegrationTests/*.csproj` referencing Testcontainers; a parallel idempotency test |
| 17 | `AddRateLimiter` with a token bucket; `Retry-After`; a daily-quota upsert (`ON CONFLICT`); `docs/openapi/sender-v1.json` committed |
| 18 | `src/NotificationWorker/*.csproj` in the `.sln`; `AddWindowsService`; validated options |
| 19 | `SKIP LOCKED` in worker code; a concurrent-claim integration test |
| 20 | MailKit referenced by the worker; an `IEmailSender` implementation setting `MessageId` |
| 21 | A `BackgroundService` processing loop; `Parallel.ForEachAsync` (or equivalent bounded concurrency); `ExecuteUpdateAsync` compare-and-set |
| 22 | The worker sets `Retrying` with `NextAttemptAt` from the retry policy; `DeadLettered` on exhaustion; a `NotificationAttempt` written for each attempt |
| 23 | A retention background service; `scripts/deploy/Deploy-Worker.ps1`; a crash-recovery test; ask the user to confirm a real email arrived from production |
| 24 | `src/NotificationWebApi/*.csproj`; an Identity DbContext; login, logout and me endpoints; API key create and revoke endpoints |
| 25 | Template version publish and deprecate endpoints; a test that published versions are immutable |
| 26 | A preview endpoint; an admin notifications search with keyset paging; a detail endpoint with attempts |
| 27 | A stats endpoint using `percentile_cont` |
| 28 | `load-tests/scenarios/smoke.js` and `accept-rate.js` |
| 29 | `docs/performance.md` with results; `load-tests/results/*.json` |
| 30 | `git tag` contains `v0.1.0`; the README has the pitch, diagram and quickstart; frontend build wired into Web API `wwwroot` (the frontend may be in a sibling `kalabit-frontend` repo) |
