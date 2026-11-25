using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kalabit");

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    keycloak_client_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    requests_per_minute = table.Column<int>(type: "integer", nullable: false),
                    daily_quota = table.Column<int>(type: "integer", nullable: false),
                    is_synthetic = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                    table.CheckConstraint("ck_clients_daily_quota_nonnegative", "\"daily_quota\" >= 0");
                    table.CheckConstraint("ck_clients_requests_per_minute_positive", "\"requests_per_minute\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "keepalive_heartbeats",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    pinged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_keepalive_heartbeats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    prefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    key_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys", x => x.id);
                    table.CheckConstraint("ck_api_keys_key_hash_length", "octet_length(\"key_hash\") = 32");
                    table.ForeignKey(
                        name: "FK_api_keys_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "kalabit",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "client_daily_usage",
                schema: "kalabit",
                columns: table => new
                {
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usage_date = table.Column<DateOnly>(type: "date", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_daily_usage", x => new { x.client_id, x.usage_date });
                    table.CheckConstraint("ck_client_daily_usage_count_nonnegative", "\"count\" >= 0");
                    table.ForeignKey(
                        name: "FK_client_daily_usage_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "kalabit",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_identities",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    idempotency_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_identities", x => x.id);
                    table.UniqueConstraint("ak_notification_identities_id_created_at", x => new { x.id, x.created_at });
                    table.ForeignKey(
                        name: "FK_notification_identities_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "kalabit",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_templates_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "kalabit",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "template_versions",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    subject_template = table.Column<string>(type: "text", nullable: false),
                    html_body_template = table.Column<string>(type: "text", nullable: false),
                    text_body_template = table.Column<string>(type: "text", nullable: false),
                    variables_schema = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_versions", x => x.id);
                    table.CheckConstraint("ck_template_versions_published_at", "\"status\" = 'Draft' OR \"published_at\" IS NOT NULL");
                    table.CheckConstraint("ck_template_versions_version_number_positive", "\"version_number\" > 0");
                    table.ForeignKey(
                        name: "FK_template_versions_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "kalabit",
                        principalTable: "templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lease_owner = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_failure_class = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    finalized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    encrypted_payload_envelope = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => new { x.id, x.created_at });
                    table.CheckConstraint("ck_notifications_attempt_count_nonnegative", "\"attempt_count\" >= 0");
                    table.CheckConstraint("ck_notifications_lease_fields", "(\"lease_owner\" IS NULL) = (\"lease_expires_at\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_notifications_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "kalabit",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_notification_identities_id_created_at",
                        columns: x => new { x.id, x.created_at },
                        principalSchema: "kalabit",
                        principalTable: "notification_identities",
                        principalColumns: new[] { "id", "created_at" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_template_versions_template_version_id",
                        column: x => x.template_version_id,
                        principalSchema: "kalabit",
                        principalTable: "template_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_attempts",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    outcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    smtp_status_code = table.Column<int>(type: "integer", nullable: true),
                    failure_class = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    worker_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    duration_milliseconds = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_attempts", x => x.id);
                    table.CheckConstraint("ck_notification_attempts_attempt_number_positive", "\"attempt_number\" > 0");
                    table.CheckConstraint("ck_notification_attempts_duration_nonnegative", "\"duration_milliseconds\" >= 0");
                    table.ForeignKey(
                        name: "FK_notification_attempts_notifications_notification_id_notific~",
                        columns: x => new { x.notification_id, x.notification_created_at },
                        principalSchema: "kalabit",
                        principalTable: "notifications",
                        principalColumns: new[] { "id", "created_at" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_recipients",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recipient_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    masked_address = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    encrypted_address_envelope = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_recipients", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_recipients_notifications_notification_id_notif~",
                        columns: x => new { x.notification_id, x.notification_created_at },
                        principalSchema: "kalabit",
                        principalTable: "notifications",
                        principalColumns: new[] { "id", "created_at" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "kalabit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    envelope = table.Column<string>(type: "jsonb", nullable: false),
                    headers = table.Column<string>(type: "jsonb", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_outbox_messages_notifications_notification_id_notification_~",
                        columns: x => new { x.notification_id, x.notification_created_at },
                        principalSchema: "kalabit",
                        principalTable: "notifications",
                        principalColumns: new[] { "id", "created_at" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_client_id",
                schema: "kalabit",
                table: "api_keys",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ux_api_keys_prefix",
                schema: "kalabit",
                table: "api_keys",
                column: "prefix",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_clients_keycloak_client_id",
                schema: "kalabit",
                table: "clients",
                column: "keycloak_client_id",
                unique: true,
                filter: "\"keycloak_client_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_clients_name",
                schema: "kalabit",
                table: "clients",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_notification_attempts_notification_attempt_number",
                schema: "kalabit",
                table: "notification_attempts",
                columns: new[] { "notification_id", "notification_created_at", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_notification_identities_client_idempotency_key",
                schema: "kalabit",
                table: "notification_identities",
                columns: new[] { "client_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_recipients_notification_id_notification_create~",
                schema: "kalabit",
                table: "notification_recipients",
                columns: new[] { "notification_id", "notification_created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notification_recipients_recipient_hash",
                schema: "kalabit",
                table: "notification_recipients",
                column: "recipient_hash");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_client_id_created_at",
                schema: "kalabit",
                table: "notifications",
                columns: new[] { "client_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_correlation_id",
                schema: "kalabit",
                table: "notifications",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_due",
                schema: "kalabit",
                table: "notifications",
                column: "next_attempt_at",
                filter: "\"status\" IN ('Queued', 'Retrying')");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_expired_leases",
                schema: "kalabit",
                table: "notifications",
                column: "lease_expires_at",
                filter: "\"status\" = 'Sending'");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_template_version_id",
                schema: "kalabit",
                table: "notifications",
                column: "template_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_notification_id_notification_created_at",
                schema: "kalabit",
                table: "outbox_messages",
                columns: new[] { "notification_id", "notification_created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_unpublished_created_at",
                schema: "kalabit",
                table: "outbox_messages",
                column: "created_at",
                filter: "\"published_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_template_versions_template_id_version_number",
                schema: "kalabit",
                table: "template_versions",
                columns: new[] { "template_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_templates_client_id_template_key",
                schema: "kalabit",
                table: "templates",
                columns: new[] { "client_id", "template_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_keys",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "client_daily_usage",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "keepalive_heartbeats",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "notification_attempts",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "notification_recipients",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "notification_identities",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "template_versions",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "templates",
                schema: "kalabit");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "kalabit");
        }
    }
}
