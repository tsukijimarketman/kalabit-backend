# Kalabit

*Kalabit* is Filipino for a gentle tap on the shoulder: the nudge that says "hey, look at this". That is what this system does for applications. Kalabit is a notification platform that accepts email requests over a simple HTTP API, queues them safely, sends them in the background, and lets you see what happened to every message.

> **Status:** early development. The backend is being built week by week from the [build plan](docs/build-plan.md). Sections below describe the target setup and will fill in as each milestone lands.

## What the backend does

1. A client app calls the Sender API with a template and some data.
2. The request is stored durably in PostgreSQL, and the API answers `202 Accepted`.
3. A background worker moves it through Kafka, renders the template and sends the email.
4. Failures are retried with backoff, and messages that cannot be sent end up in a dead-letter queue.
5. Every step is tracked, so you can ask "where is my email?" and get an answer.

## Technologies

| Area | Technology | Why |
| --- | --- | --- |
| Runtime | .NET 10 (LTS) and C# 14 | Long-term support until November 2028 |
| Web APIs | ASP.NET Core with controllers | Mature, well documented, and a good fit for REST |
| API contract | OpenAPI via `Microsoft.AspNetCore.OpenApi` | Microsoft's built-in generator, no extra dependency |
| API documentation | [Scalar](https://scalar.com) (`Scalar.AspNetCore`) | Browse the API and send test requests from the browser |
| Database | PostgreSQL, EF Core, Npgsql | One reliable store for state, outbox and analytics |
| Messaging | Apache Kafka (KRaft mode), Confluent.Kafka | Asynchronous processing and retries |
| Email | MailKit, with Mailpit for local testing | Real SMTP without sending real email |
| Templates | Fluid (Liquid) | Safe, sandboxed email templates |
| Authentication | Keycloak (OAuth 2.0 / OpenID Connect) | Standards-based sign-in for apps and users |
| Logging | Serilog (structured JSON) | Searchable logs with correlation IDs |
| Testing | xUnit, Testcontainers | Unit tests and tests against real PostgreSQL and Kafka |
| Load testing | k6 | Measures how much traffic Kalabit can sustain |
| Local environment | Docker Compose | One command starts the supporting services |

Everything above is free and open source.

## Project layout

```text
kalabit/
├── src/
│   ├── Lib/                     # shared contracts, DTOs, enums
│   ├── Domain/                  # pure business rules (state machine, retry logic)
│   ├── Data/                    # EF Core and PostgreSQL
│   ├── Templating/              # email template rendering
│   ├── Migrator/                # applies database migrations
│   ├── NotificationSenderApi/   # public API: accept and store requests
│   ├── NotificationWebApi/      # admin API for the dashboard
│   ├── NotificationWorker/      # outbox relay, Kafka consumers, email sending
│   └── LoadTestWorker/          # runs k6 and stores results
├── tests/
├── load-tests/                  # k6 scripts
├── infra/                       # Keycloak, Kafka and monitoring configuration
├── docs/                        # ADRs, runbooks, build plan
├── frontend/                    # React dashboard (later)
└── docker-compose.yml
```

## Getting started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download), [Docker](https://www.docker.com/) and Git.

```bash
git clone <repository-url>
cd kalabit

# start PostgreSQL, Kafka and Mailpit
docker compose up -d

# run the Sender API
dotnet run --project src/NotificationSenderApi
```

When the API is running in Development mode:

| What | Where |
| --- | --- |
| API reference (Scalar) | `/scalar` |
| OpenAPI document | `/openapi/v1.json` |
| Mailpit inbox (caught emails) | http://localhost:8025 |

The OpenAPI document and Scalar are only enabled in Development and staging, never in production.

## Running the tests

```bash
dotnet test
```

Integration tests start PostgreSQL and Kafka in containers, so Docker must be running.

## License

To be decided.