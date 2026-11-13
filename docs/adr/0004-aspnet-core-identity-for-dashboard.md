# ADR-0004: Use ASP.NET Core Identity with cookies for the dashboard

- **Status:** Accepted
- **Date:** 2026-11-13
- **PRD reference:** Key decision 7 (p.4); Dashboard (p.14–16); Roles matrix (p.21)

## Context

Operators sign in to the dashboard to manage clients, API keys and templates and to search notifications. The PRD uses Keycloak with Authorization Code + PKCE through a backend-for-frontend, so no tokens are stored in the browser. That aim is right, but Keycloak is too heavy for v0.1 (see ADR-0003).

## Decision

The Web API uses **ASP.NET Core Identity** with **cookie authentication**:

- The cookie is `HttpOnly`, `Secure` and `SameSite=Strict`. The React dashboard is served from the same site (Web API `wwwroot`), so the cookie is sent automatically and JavaScript never handles a token.
- Roles (for example `Admin` and `Viewer`) implement the PRD roles matrix through authorization policies.
- Identity has its own `DbContext` and tables in the `kalabit` schema, separate from the notification model.
- Endpoints: login, logout, and "me" (the current user and roles).

## Consequences

**Positive**

- Free, built into ASP.NET Core and in-process. No extra service.
- Keeps the PRD's security goal of no tokens in browser storage.
- Password hashing, lockout and security stamps come from a well-tested library instead of our own code.

**Negative**

- No single sign-on and no external identity providers in v0.1.
- Cookie authentication needs CSRF thinking. `SameSite=Strict` and same-site hosting cover most cases; antiforgery tokens are added if the dashboard ever runs on another site.
- User management (invites, password reset) is our own work.

## Revisit when

- We need single sign-on, an external identity provider, or users from more than one organisation. Then reconsider Keycloak or a hosted identity provider.
