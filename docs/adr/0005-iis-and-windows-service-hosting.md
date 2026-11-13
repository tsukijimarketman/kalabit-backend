# ADR-0005: Host the APIs on IIS and the worker as a Windows Service

- **Status:** Accepted
- **Date:** 2026-11-13
- **PRD reference:** Deployment (p.33–34); Build Plan release week (p.18–19)

## Context

The PRD deploys every service as a container with Docker Compose profiles. For v0.1 the only production machine is a Windows server with IIS, at no cost.

IIS suits request/response web apps, but it recycles application pools (on a schedule, after idle time, on configuration changes). It is not meant to host long-running background loops like the email worker. A loop inside an app pool can be stopped mid-send at any time.

## Decision

- **NotificationSenderApi** and **NotificationWebApi** (with the React build in `wwwroot`) run as **IIS sites** using the ASP.NET Core Module, in-process hosting, each in its own app pool with **No Managed Code**, start mode `AlwaysRunning`, idle time-out `0`, and preload enabled.
- **NotificationWorker** runs as a **Windows Service** (`AddWindowsService`), set to restart automatically on failure.
- Secrets and environment-specific settings come from app-pool and service **environment variables** (for example `ConnectionStrings__NotificationDatabase`), never from files in the deployment folder.
- Deploys use `app_offline.htm` to stop each site cleanly while files are copied, and PowerShell scripts in `scripts/deploy/`.
- Docker Compose remains for development and load tests (Postgres and Mailpit).

## Consequences

**Positive**

- Zero hosting cost on a machine we already control.
- Each part runs on the host built for it: IIS for HTTP, the Service Control Manager for long-running work.
- Learning IIS and Windows Services is directly useful for enterprise and consultancy work.

**Negative**

- Production is Windows-only and differs from the Linux CI runners. Path casing and line endings need care.
- No image-based rollbacks. A rollback means redeploying the previous build folder.
- One machine is a single point of failure. A restart or outage stops both the APIs and the worker. The queue survives (see ADR-0002), but delivery pauses.

## Revisit when

- We move to containers, a VPS or a cloud platform, or need more than one machine.
- Uptime matters more than cost.
