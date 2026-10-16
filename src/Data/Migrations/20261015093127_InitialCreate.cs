using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    keycloak_client_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                name: "templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "notification_identities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    payload_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    idempotency_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_identities", x => x.id);
                    table.UniqueConstraint("ak_notification_identities_id_created_at", x => new { x.id, x.created_at });
                    table.CheckConstraint("ck_notification_identities_idempotency_fields", "(\r\n    \"idempotency_key\" IS NULL\r\n    AND \"payload_hash\" IS NULL\r\n    AND \"idempotency_expires_at\" IS NULL\r\n)\r\nOR\r\n(\r\n    \"idempotency_key\" IS NOT NULL\r\n    AND \"payload_hash\" IS NOT NULL\r\n    AND \"idempotency_expires_at\" IS NOT NULL\r\n)");
                    table.ForeignKey(
                        name: "FK_notification_identities_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "template_versions",
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
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_versions", x => x.id);
                    table.CheckConstraint("ck_template_versions_version_number_positive", "\"version_number\" > 0");
                    table.ForeignKey(
                        name: "FK_template_versions_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                CREATE TABLE notifications (
                    id uuid NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    client_id uuid NOT NULL,
                    template_version_id uuid NOT NULL,
                    correlation_id uuid NOT NULL,
                    status character varying(20) NOT NULL,
                    expires_at timestamp with time zone,
                    encrypted_payload_envelope bytea,
                    CONSTRAINT "PK_notifications" PRIMARY KEY (id, created_at),
                    CONSTRAINT "FK_notifications_clients_client_id"
                        FOREIGN KEY (client_id) REFERENCES clients (id) ON DELETE RESTRICT,
                    CONSTRAINT "FK_notifications_notification_identities_id_created_at"
                        FOREIGN KEY (id, created_at)
                        REFERENCES notification_identities (id, created_at) ON DELETE RESTRICT,
                    CONSTRAINT "FK_notifications_template_versions_template_version_id"
                        FOREIGN KEY (template_version_id)
                        REFERENCES template_versions (id) ON DELETE RESTRICT
                ) PARTITION BY RANGE (created_at);

                DO $partition_setup$
                DECLARE
                    month_start timestamp without time zone :=
                        date_trunc('month', CURRENT_TIMESTAMP AT TIME ZONE 'UTC');
                    partition_start timestamp with time zone;
                    partition_end timestamp with time zone;
                    partition_name text;
                    month_offset integer;
                BEGIN
                    FOR month_offset IN 0..12 LOOP
                        partition_start :=
                            (month_start + make_interval(months => month_offset)) AT TIME ZONE 'UTC';
                        partition_end :=
                            (month_start + make_interval(months => month_offset + 1)) AT TIME ZONE 'UTC';
                        partition_name :=
                            'notifications_' ||
                            to_char(partition_start AT TIME ZONE 'UTC', 'YYYY_MM');

                        EXECUTE format(
                            'CREATE TABLE %I PARTITION OF notifications FOR VALUES FROM (%L) TO (%L)',
                            partition_name,
                            partition_start,
                            partition_end);
                    END LOOP;
                END
                $partition_setup$;
                """);

            migrationBuilder.CreateTable(
                name: "notification_attempts",
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
                        principalTable: "notifications",
                        principalColumns: new[] { "id", "created_at" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_recipients",
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
                        principalTable: "notifications",
                        principalColumns: new[] { "id", "created_at" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
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
                        principalTable: "notifications",
                        principalColumns: new[] { "id", "created_at" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_clients_keycloak_client_id",
                table: "clients",
                column: "keycloak_client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_notification_attempts_notification_attempt_number",
                table: "notification_attempts",
                columns: new[] { "notification_id", "notification_created_at", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_notification_identities_client_idempotency_key",
                table: "notification_identities",
                columns: new[] { "client_id", "idempotency_key" },
                unique: true,
                filter: "\"idempotency_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_notification_recipients_notification_id_notification_create~",
                table: "notification_recipients",
                columns: new[] { "notification_id", "notification_created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notification_recipients_recipient_hash",
                table: "notification_recipients",
                column: "recipient_hash");

            migrationBuilder.CreateIndex(
                name: "ix_notification_correlation_id",
                table: "notifications",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_client_id_created_at",
                table: "notifications",
                columns: new[] { "client_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_template_version_id",
                table: "notifications",
                column: "template_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_notification_id_notification_created_at",
                table: "outbox_messages",
                columns: new[] { "notification_id", "notification_created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_unpublished_created_at",
                table: "outbox_messages",
                column: "created_at",
                filter: "\"published_at\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_template_versions_template_id_version_number",
                table: "template_versions",
                columns: new[] { "template_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_templates_template_key",
                table: "templates",
                column: "template_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_attempts");

            migrationBuilder.DropTable(
                name: "notification_recipients");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "notification_identities");

            migrationBuilder.DropTable(
                name: "template_versions");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "templates");
        }
    }
}
