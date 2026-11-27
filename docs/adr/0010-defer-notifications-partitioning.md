# ADR-0010: Defer monthly partitioning of the notifications table

- **Status:** Accepted
- **Date:** 2026-11-25
- **PRD reference:** Data model and retention (p.26), where `notifications` is "partitioned monthly"; 30-day plan §9, scaling step 7

## Context

The PRD partitions `notifications` by month so that old data can be removed by dropping a whole partition instead of deleting millions of rows.

The first `InitialCreate` migration did this with hand-written SQL. It created `notifications` with `PARTITION BY RANGE (created_at)` and then 13 monthly partitions. Three problems showed up when the v0.1 schema was rebuilt on Day 3:

1. **It would stop accepting data.** Only 13 partitions were created, there was no default partition, and no job created new ones. Thirteen months after deployment, every insert would fail with `no partition of relation "notifications" found for row`.
2. **It ignored the `kalabit` schema.** The raw SQL created `notifications` without naming a schema, so it did not follow `HasDefaultSchema("kalabit")`. EF's model and the real database would disagree about where the table lives.
3. **It solves a problem v0.1 does not have.** v0.1 runs on the Supabase free tier, which caps the database at 500 MB. The table stays far below the size where partitioning pays off. Payloads are erased 7 days after a notification is finalized, and the retention job keeps the row count small. Each extra partition also adds its own copy of every index.

## Decision

- `notifications` is an **ordinary table** in v0.1. The regenerated `InitialCreate` contains no raw partitioning SQL.
- We keep the composite primary key (`id`, `created_at`) and the matching foreign keys from `notification_identities`, `notification_recipients` and `notification_attempts`. PostgreSQL requires the partition key to be part of every unique constraint, so the table is still shaped for partitioning later.
- Retention (Day 23) deletes rows in batches instead of dropping partitions.

## Consequences

**Positive**

- No time bomb: inserts can't start failing on a calendar date.
- The whole schema comes from the EF model, so `migrations add` produces correct diffs and the model matches the database.
- Fewer objects in a 500 MB database, and one less moving part to operate.

**Negative**

- Retention deletes rows instead of dropping partitions. Large `DELETE`s create dead tuples and need autovacuum to keep up. That's fine at v0.1 volumes, but it gets slow at millions of rows.
- Moving to partitions later isn't a simple `ALTER TABLE`. It means creating a partitioned table, copying the data in batches and swapping the tables, plus a job (or `pg_partman`) that creates future partitions and a `DEFAULT` partition as a safety net.
- This is a documented gap against the PRD.

## Revisit when

- `notifications` grows past about 1 million rows, **or**
- retention deletes take long enough to show up in the worker's latency or in autovacuum activity, **or**
- the database approaches its storage limit and moving to a paid tier is already on the table.
