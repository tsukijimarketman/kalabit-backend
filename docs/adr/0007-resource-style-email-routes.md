# ADR-0007: Use resource-style routes `POST /api/v1/emails` and `GET /api/v1/emails/{id}`

- **Status:** Accepted
- **Date:** 2026-11-13
- **PRD reference:** Key decision 13 (p.5); Email API (p.8–9); Error contract (p.9–10)

## Context

The PRD keeps `POST /api/v1.1/Email/SendEmail` so that existing callers keep working. Kalabit has **no existing callers**: this is a new product, so there is no compatibility to protect. The old route is RPC-style: it uses a verb in the path, a minor version in the URL, and controller-shaped names.

## Decision

- Accept an email with **`POST /api/v1/emails`**. It returns **`202 Accepted`** with the notification id and a `Location` header pointing to the status resource.
- Read its status with **`GET /api/v1/emails/{id}`**.
- The version is a **major-only path segment** (`v1`). Changes inside v1 are additive only: new optional fields and new endpoints. A breaking change means `v2`, with v1 kept running during a published deprecation period.
- The rest of PRD decision 13 still applies: the `Idempotency-Key` header and RFC 9457 problem-details errors.

## Consequences

**Positive**

- Routes follow common REST conventions (lowercase, plural nouns, HTTP methods as the verbs), so integrators can guess them.
- `202` plus `Location` tells clients exactly where to check delivery status.
- A simple, predictable versioning rule.

**Negative**

- Diverges from the PRD's route, so the PRD examples and any earlier sketches must be read with this ADR in mind.
- The `202` response means "accepted", not "sent". Clients have to understand that delivery happens later (documented in the API reference).

## Revisit when

- Never for the route shape. Introduce `/api/v2` only for a breaking change.
