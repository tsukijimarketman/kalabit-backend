# ADR-0002: Use PostgreSQL as the work queue instead of Kafka for v0.1

- **Status:** Accepted
- **Date:** 2026-11-13
- **PRD reference:** Key decisions 1, 3, 4 and 5 (p.4); Kafka topology (p.7–8); Reliability (p.11–13)

## Context

The PRD design accepts a request by writing the notification and an outbox row in one transaction. A relay then publishes to Kafka, and a consumer sends the email, with retry topics and a dead-letter topic. That design is sound, but Kafka has to run somewhere. v0.1 has a zero-cost budget: an IIS server, a Windows Service and the Supabase free tier. There is no free, reliable way to run a Kafka cluster next to that.

The guarantees we actually need are: a `202 Accepted` means the request is stored durably, every accepted email is processed at least once, failures are retried with backoff, and a crashed worker does not lose work.

## Decision

**The notification row is the job.** The worker claims due rows straight from PostgreSQL:

- The claim query selects rows whose status is `Queued` or `Retrying` and whose `next_attempt_at` is due, using `FOR UPDATE SKIP LOCKED`. Several worker threads, or several workers, never claim the same row, and never wait on each other.
- A claimed row gets a **lease** (`lease_owner` and `lease_expires_at`). If a worker crashes mid-send, the lease expires and another worker reclaims the row. This is how we keep at-least-once delivery.
- Retries set `next_attempt_at` from the retry policy instead of using delay topics. Rows that run out of attempts become `DeadLettered`.
- The `outbox_messages` table stays in the schema but nothing writes to it yet, so moving to Kafka later doesn't need a schema redesign.

PRD decision 2 is unchanged: at-least-once delivery, idempotent state transitions, and no exactly-once claim.

## Consequences

**Positive**

- No broker to host, monitor or pay for. Accepting a request is a single database transaction.
- One store for state and queue, so there is no dual-write problem and nothing can be lost "between the database and the broker".
- `SKIP LOCKED` queues are a well-known pattern that comfortably covers v0.1 demand. The Day 29 load test puts a measured number on it.
- The worker's polling also keeps the Supabase free project from pausing.

**Negative**

- The worker polls, which adds a little latency (up to one poll interval) and a steady background load on a small database.
- There is only one consumer type. Fan-out to other channels (SMS, webhooks) would mean more polling queries, or a broker.
- Retry scheduling, leases and dead-lettering are our own code rather than broker features, so they need their own tests (crash-recovery test on Day 23).
- Connections are limited on the free tier, so each process uses a capped pool (see ADR-0006).

## Revisit when

- Claim queries become a major share of database CPU, or measured throughput needs exceed what one PostgreSQL instance sustains.
- We need a second, independent consumer type (for example SMS or webhooks).
- There is a budget for a managed broker. The unused outbox table is the migration path.
