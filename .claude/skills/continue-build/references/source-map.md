# Source map: where each topic lives

Extract only the pages you need:
`pdftotext -enc UTF-8 -layout -f <first> -l <last> "Notification_Platform_PRD_v2.0.pdf" -`

## PRD v2.0 (39 pages)

| Topic | Pages |
|---|---|
| What changed from v1, product overview, goals, personas, out of scope | 1–3 |
| Success criteria | 3 |
| Key decisions (14 ADR candidates) | 4–5 |
| Architecture, components | 5–7 |
| Kafka topology, message contract (claim check) | 7–8 |
| Notification lifecycle and states | 8 |
| Email API: endpoint, request and response | 8–9 |
| Request processing order | 9 |
| Error contract (400–503, RFC 9457, idempotent replay) | 9–10 |
| Rate limiting and quotas | 10 |
| Templates (versions, Fluid sandbox, categories) | 10–11 |
| SMTP sending rules (TLS, timeouts, 2xx/4xx/5xx, Message-ID, header injection) | 11 |
| Suppression list | 11 |
| Reliability guarantees, failure classes, retry tiers | 11–13 |
| Expected behaviour under failure (chaos table) | 13 |
| Dashboard modules, real time, frontend stack, UX and accessibility | 14–16 |
| Load testing: principles, test types, capacity definition | 16–18 |
| Load testing: execution flow, safety controls, metrics, regression | 18–20 |
| Security: identity, roles matrix | 21 |
| Secrets and cryptography (envelope encryption, HMAC pepper) | 21–22 |
| Threat model highlights | 22–23 |
| Privacy, email standards (SPF/DKIM/DMARC, RFC 8058), audit | 23–24 |
| SLOs, telemetry (`notif_*` metrics), alerts | 24–25 |
| Data model: core tables, conventions, retention | 25–28 |
| Tech stack, licence audit, excluded libraries | 28–31 |
| Engineering standards, test strategy, CI pipeline, docs as code | 31–33 |
| Deployment: Compose profiles, environments, configuration, backup | 33–34 |
| Roadmap phases, MoSCoW priorities | 34–37 |
| Definition of done | 37 |
| Portfolio packaging, interview talking points | 37–38 |
| Risks and open questions | 38–39 |

## Build Plan (19 pages)

| Topic | Pages |
|---|---|
| .NET 10 and OpenAPI/Scalar decisions | 1–2 |
| Solution structure, what changed, dependency rules | 2–5 |
| How the plan works, daily routine | 5–7 |
| Week 1: foundations (tools, CI, Compose, Kafka) | 7–8 |
| Week 2: domain and data (state machine, entities, Migrator, Templating) | 8 |
| Week 3: accept path (skeleton, validation, transaction, idempotency, integration tests) | 9 |
| Week 4: hardening (template checks, rate limit, quota, OpenAPI) | 9–10 |
| Week 5: relay and consumer | 10–11 |
| Week 6: sending and retrying (MailKit, outcomes, classification, retry tiers) | 11–12 |
| Week 7: dead letters, suppression, breaker, chaos tests | 12–13 |
| Week 8: security (Keycloak, JWT, authorization matrix, secrets, hygiene) | 13 |
| Week 9: admin API part 1 (roles, clients, templates, preview, audit) | 13–14 |
| Week 10: admin API part 2 (sender profiles, suppression, search, replay, retention) | 14–15 |
| Week 11: analytics, SignalR, OpenTelemetry, metrics, alerts | 15 |
| Weeks 12–14: k6, LoadTestWorker, capacity | 16–18 |
| Frontend track, release week, stretch goals | 18–19 |

## 30-day plan day → PRD / Build Plan cross-reference

| Day | Topic | PRD pages | Build Plan pages | Plan ADR |
|---|---|---|---|---|
| 1 | Cleanup, upgrade-proofing | 28, 31 | 1–2, 5 | ADR-001 |
| 2 | CI, ADRs, Supabase | 4–5, 32 | 7 | all |
| 3 | v0.1 schema, Migrator | 25–28 | 8 | 002, 006 |
| 4 | State machine | 8, 12 | 8 | 002 |
| 5 | Retry policy | 12–13 | 11–12 | — |
| 6 | Failure classifier | 11–12 | 11 | — |
| 7 | API key primitives | 21 | 13 | 003 |
| 8 | Logging, health checks | 24–25 | 9 | — |
| 9 | Deploy to IIS and Supabase, keepalive | 33–34 | 18–19 | 005, 006 |
| 10 | API key authentication handler | 21–22 | 13 | 003 |
| 11 | Contract, problem details | 8–10 | 9 | 007 |
| 12 | Correlation ID | 25 | 9 | — |
| 13 | Payload encryption, hashing | 21–23 | 13 | — |
| 14 | Template lookup, trial render | 10–11 | 9 | 008 |
| 15 | Masking, handler skeleton | 15, 23 | — | — |
| 16 | Accept path, idempotency, integration tests | 9–12 | 9 | — |
| 17 | Rate limit, quota, OpenAPI | 10 | 9–10 | 009 |
| 18 | Worker skeleton | 6–7 | 10 | 005 |
| 19 | Claim query (SKIP LOCKED) | 11–12 | 11 | 002 |
| 20 | MailKit sender | 11 | 11 | — |
| 21 | Processing loop | 11–13 | 11 | — |
| 22 | Failures and retries | 12 | 11–12 | — |
| 23 | Hardening, retention, Windows Service, real SMTP | 13, 27–28 | 12, 15 | 005 |
| 24 | Web API, Identity | 14–15, 21 | 13–14 | 004 |
| 25 | Templates admin | 10, 14 | 14 | — |
| 26 | Preview, search | 14 | 14 | — |
| 27 | Stats, percentiles | 14, 24 | 15 | — |
| 28 | k6 scripts | 16–18 | 16 | — |
| 29 | Baseline, performance report | 17–18, 37–38 | 16 | — |
| 30 | Dashboard, launch | 14–16, 37–38 | 18–19 | — |
