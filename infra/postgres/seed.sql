BEGIN;

INSERT INTO clients (
    id,
    created_at,
    keycloak_client_id,
    is_active,
    requests_per_minute,
    daily_quota,
    is_synthetic
)
VALUES (
    '00000000-0000-7000-8000-000000000001',
    now(),
    'local-demo-client',
    TRUE,
    60,
    1000,
    TRUE
)
ON CONFLICT (keycloak_client_id) DO NOTHING;

INSERT INTO templates (
    id,
    created_at,
    template_key,
    is_active
)
VALUES (
    '00000000-0000-7000-8000-000000000002',
    now(),
    'welcome-email-demo',
    TRUE
)
ON CONFLICT (template_key) DO NOTHING;

INSERT INTO template_versions (
    id,
    created_at,
    template_id,
    version_number,
    subject_template,
    html_body_template,
    text_body_template,
    variables_schema,
    status,
    content_hash
)
VALUES (
    '00000000-0000-7000-8000-000000000003',
    now(),
    '00000000-0000-7000-8000-000000000002',
    1,
    $subject$Hello {{ firstName }}$subject$,
    $html$<!doctype html>
<html><body><h1>Hello {{ firstName }}!</h1><p>This is a demo notification.</p></body></html>$html$,
    $text$Hello {{ firstName }}!

This is a demo notification.$text$,
    '{
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "type": "object",
        "properties": {
            "firstName": { "type": "string" }
        },
        "required": ["firstName"],
        "additionalProperties": false
    }'::jsonb,
    'Published',
    '5fb4f562da69a5268c675a297f3e4f5f581d4acf8e06c992b2f843ee33d68e74'
)
ON CONFLICT (template_id, version_number) DO NOTHING;

COMMIT;
