# ADR-0003: Authenticate client systems with API keys instead of Keycloak

- **Status:** Accepted
- **Date:** 2026-11-13
- **PRD reference:** Key decision 7 (p.4); Security, identity and roles (p.21); Secrets and cryptography (p.21–22)

## Context

The PRD uses Keycloak with the OAuth 2.0 client-credentials flow for calling applications. Keycloak is a separate Java service with its own database, upgrades and backups. On a single free IIS box it would cost more to run than the rest of Kalabit put together.

Kalabit's callers are back-end systems sending transactional email. Widely used developer APIs such as Stripe, Postmark, Resend and SendGrid authenticate these with long-lived API keys, so integrators already know the pattern.

## Decision

Clients authenticate with **`Authorization: Bearer kb_<key>`**.

- Keys are generated with `RandomNumberGenerator` (at least 256 bits), have a recognisable `kb_` prefix, and are **shown once** when created.
- We store only a **SHA-256 hash** of the key and a short, non-secret lookup prefix. We find the key by prefix, then compare hashes with `CryptographicOperations.FixedTimeEquals`, so timing doesn't reveal how much matched.
- Each key belongs to one client, can be revoked, and records when it was last used. Permissions are granted with scopes (for example `notifications:send`) and checked with an authorization policy.
- This runs in a custom `AuthenticationHandler` inside the Sender API, so the rest of the pipeline uses normal ASP.NET Core authorization.

## Consequences

**Positive**

- No extra service to run. Authentication is one indexed lookup and one hash.
- Easy for integrators: one header, no token endpoint, and no token refresh.
- A leaked database doesn't leak usable keys, because only hashes are stored.
- The `kb_` prefix makes keys easy to spot in logs and with secret scanners such as GitHub secret scanning.

**Negative**

- Keys are long-lived. Rotation is manual: create a new key, deploy it, revoke the old one.
- No standard token features: no expiry claims and no delegated or user-level access.
- We own security-sensitive code, so it needs tests for wrong keys, revoked keys and timing-safe comparison (Days 7 and 10).

## Revisit when

- A customer requires OAuth 2.0 or OpenID Connect. Add it as a **second authentication scheme** alongside API keys rather than replacing them.
- We need short-lived or per-user credentials.
