# Data model

This diagram shows the core notification, client, template, and delivery tables.
`PK` means primary key, `FK` means foreign key, and `UK` means unique key.

```mermaid
erDiagram
    CLIENTS {
        uuid id PK
        string keycloak_client_id UK
        bool is_active
        int requests_per_minute
        int daily_quota
    }

    TEMPLATES {
        uuid id PK
        string template_key UK
        bool is_active
    }

    TEMPLATE_VERSIONS {
        uuid id PK
        uuid template_id FK
        int version_number
        string status
        string subject_template
        string html_body_template
        string text_body_template
        jsonb variables_schema
        string content_hash
    }

    NOTIFICATION_IDENTITIES {
        uuid id PK
        timestamptz created_at
        uuid client_id FK
        string idempotency_key
        string payload_hash
    }

    NOTIFICATIONS {
        uuid id PK
        timestamptz created_at PK
        uuid client_id FK
        uuid template_version_id FK
        string status
        uuid correlation_id
    }

    NOTIFICATION_RECIPIENTS {
        uuid id PK
        uuid notification_id FK
        timestamptz notification_created_at FK
        bytea recipient_hash
        string masked_address
    }

    NOTIFICATION_ATTEMPTS {
        uuid id PK
        uuid notification_id FK
        timestamptz notification_created_at FK
        int attempt_number
        string outcome
    }

    OUTBOX_MESSAGES {
        uuid id PK
        uuid notification_id FK
        timestamptz notification_created_at FK
        string topic
        jsonb envelope
        timestamptz published_at
    }

    CLIENTS ||--o{ NOTIFICATION_IDENTITIES : "owns"
    CLIENTS ||--o{ NOTIFICATIONS : "sends"
    TEMPLATES ||--o{ TEMPLATE_VERSIONS : "has versions"
    TEMPLATE_VERSIONS ||--o{ NOTIFICATIONS : "used by"
    NOTIFICATION_IDENTITIES ||--o| NOTIFICATIONS : "identifies"
    NOTIFICATIONS ||--o{ NOTIFICATION_RECIPIENTS : "has recipients"
    NOTIFICATIONS ||--o{ NOTIFICATION_ATTEMPTS : "has attempts"
    NOTIFICATIONS ||--o{ OUTBOX_MESSAGES : "queued as"
```

The symbol at each end describes how many records on that side can participate:

| Symbol | Meaning |
|---|---|
| `|` | One |
| `o` | Zero (optional) |
| `{` | Many (crow's foot) |
| `||` | Exactly one |
| `o|` | Zero or one |
| `|{` | One or many |
| `o{` | Zero or many |

Read a relationship from either end. For example, `CLIENTS ||--o{ NOTIFICATIONS` means each notification has exactly one client, while a client may have zero or many notifications.
