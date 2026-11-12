# Progress log

The `/continue-build` skill appends to this file. Newest entries are at the bottom. Day 1 is Fri 2026-11-13; the plan is in [30-day-plan.md](30-day-plan.md).

## 2026-11-12 · plan · Day 0
- Evidence: the repo is at about Build Plan Week 3, Day 1 (schema, state machine, renderer, health endpoints, contracts started). There is no CI, and the Migrator doesn't apply migrations.
- In progress: nothing
- Next: Day 1: clean-up and upgrade-proofing (delete the leftovers, move every target framework into `Directory.Build.props`, remove the `LangVersion` pin)
- Build/tests: not run

## 2026-11-12 · resume · Day 1 (started early, one day ahead of the calendar)
- Evidence: nothing from Day 1 is done yet. Still present: 3× Class1.cs, the WeatherForecast files, TargetFramework in 7 csproj files, the LangVersion pin, mailpit:latest. Missing: .env.example and dotnet-outdated-tool.
- In progress: the plan and skill files are uncommitted (commit them before the Day 1 work)
- Next: Step 0 (commit the docs, take a build and test baseline), then Steps 1–8 of the Day 1 guide
- Build/tests: not run (the user declined the automated run; they'll take the baseline themselves)

## 2026-11-12 · wrap · Day 1
- Evidence: leftovers deleted; target framework only in Directory.Build.props; LangVersion removed; mailpit pinned to v1.33; dotnet-outdated-tool 4.9.0 added; .env is still ignored
- Left: fix the `POSGRES_DB` typo in .env.example; make the .http `{{host}}` match `@NotificationSenderApi_HostAddress`; then commit (split into 4 commits suggested) and push
- Next: Day 2 (Sat 14 Nov): CI workflow, Dependabot (include the docker ecosystem), ADR-001 to ADR-009, Supabase project
- Build/tests: build OK with 0 warnings; 9/9 unit tests pass
