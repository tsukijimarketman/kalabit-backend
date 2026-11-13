# Architecture decision records

Each file records one decision: the context, the decision, its consequences, and the evidence that would make us change it. Accepted ADRs are not edited. If a decision changes, write a new ADR that supersedes the old one, and set the old one's status to "Superseded by ADR-xxxx".

ADRs 0002 to 0006 deliberately differ from the [PRD](../../Notification_Platform_PRD_v2.0.pdf) so that v0.1 can run at zero cost. Each one names the trigger for moving back toward the PRD design.

| ADR | Decision | Status |
|---|---|---|
| [0001](0001-target-dotnet-10-lts.md) | Target .NET 10 LTS and upgrade only from LTS to LTS | Accepted |
| [0002](0002-postgres-as-the-queue.md) | Use PostgreSQL as the work queue instead of Kafka for v0.1 | Accepted |
| [0003](0003-api-keys-for-client-authentication.md) | Authenticate client systems with API keys instead of Keycloak | Accepted |
| [0004](0004-aspnet-core-identity-for-dashboard.md) | Use ASP.NET Core Identity with cookies for the dashboard | Accepted |
| [0005](0005-iis-and-windows-service-hosting.md) | Host the APIs on IIS and the worker as a Windows Service | Accepted |
| [0006](0006-supabase-free-tier-postgres.md) | Use the Supabase free tier for production PostgreSQL | Accepted |
| [0007](0007-resource-style-email-routes.md) | Use resource-style routes `POST /api/v1/emails` and `GET /api/v1/emails/{id}` | Accepted |
| [0008](0008-trial-render-at-accept-time.md) | Validate template data with a trial render at accept time | Accepted |
| [0009](0009-public-api-reference-for-sender-api.md) | Publish the Sender API's OpenAPI document and Scalar reference in production | Accepted |

## Template

```markdown
# ADR-NNNN: <decision as a short sentence>

- **Status:** Proposed | Accepted | Superseded by ADR-NNNN
- **Date:** YYYY-MM-DD
- **PRD reference:** <section and page>

## Context
## Decision
## Consequences
## Revisit when
```
