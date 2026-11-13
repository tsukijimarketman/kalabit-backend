# ADR-0006: Use the Supabase free tier for production PostgreSQL

- **Status:** Accepted
- **Date:** 2026-11-13
- **PRD reference:** Key decision 6 (p.4); Data model and retention (p.25–28); Deployment and backup (p.33–34)

## Context

Kalabit needs one PostgreSQL database for state, the queue (ADR-0002) and analytics. Running Postgres ourselves on the IIS machine means owning backups, patching and disk space. Supabase offers a managed PostgreSQL free tier, but it comes with limits we have to design around.

## Decision

Production uses a **Supabase free project** in the **Singapore** region (closest to the Philippines), configured as follows.

**Connection**

- Use the **session pooler** (port 5432, user `postgres.<project-ref>`). The free tier's direct connection is IPv6-only; the pooler works over IPv4. Session mode supports transactions, `SKIP LOCKED` and advisory locks. Do not use transaction mode (port 6543).
- `SSL Mode=Require` now. Move to `VerifyFull` with Supabase's root certificate when the server is deployed. Never use `Trust Server Certificate=true`.
- `Maximum Pool Size=10` per process. Three processes use at most 30 connections, well within the free limit.
- Connection strings live in a password manager, ignored local files and server environment variables. They are never committed.

**Exposure**

- All tables live in a **`kalabit` schema**, not `public`.
- **Data API disabled**, and "automatically expose new tables" disabled. Kalabit talks to Postgres directly through Npgsql and does not need Supabase's auto-generated REST API.
- "Automatic RLS" left on. It only affects the `public` schema and acts as a safety net.

**Roles (least privilege)**

- The `postgres` owner role is used **only by the Migrator**.
- A `kalabit_app` login role with `SELECT`, `INSERT`, `UPDATE` and `DELETE` on the `kalabit` schema is used by the APIs and the worker.

**Limits we design around**

- **500 MB database size.** Retention jobs (Day 23) are required, not optional.
- **Projects pause after about a week without activity.** The worker's polling keeps it awake; a daily Task Scheduler keepalive script (`scripts/ops/`) is the safety net when the worker is stopped.
- **Never run load tests against Supabase.** Load tests use local Docker Postgres.

## Consequences

**Positive**

- Managed PostgreSQL at no cost.
- No database server to patch or back up ourselves.
- The schema and role setup is the same we'd use on any managed Postgres, so moving later is a connection-string change plus a data migration.

**Negative**

- Hard 500 MB limit, a pause risk, no point-in-time recovery, and shared resources whose performance can vary.
- Check what backups the free plan currently includes. If it has none we can download, add a scheduled `pg_dump` of the `kalabit` schema to the operations scripts (Day 23).
- An extra network hop through the pooler adds latency compared with a local database.
- Staging needs a second free project (the free tier allows two).

## Revisit when

- The database approaches 400 MB even with retention in place, or we need point-in-time recovery.
- Pooler latency or connection limits show up in the load-test results or the SLOs.
- A paid plan or a self-hosted Postgres becomes affordable.
