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

## 2026-11-12 · wrap · Day 1 ✅ DONE
- Evidence: all Day 1 fingerprints pass; `POSTGRES_DB` typo fixed; .http request resolves (hard-coded URL); committed as `5804988 clean up day 1` and pushed (main = origin/main)
- Left: learning note `docs/notes/111226.md` (optional); nit: `.http:3` hard-codes the URL instead of using `{{NotificationSenderApi_HostAddress}}`, so fix it when you next touch the file
- Next: Day 2 (Sat 14 Nov, 5h): Supabase project first, then `.github/workflows/ci.yml`, `.github/dependabot.yml` (nuget + github-actions + docker), ADR-001 to ADR-009, CI badge, make the repo public, social post #1
- Build/tests: build OK with 0 warnings; 9/9 unit tests pass

## 2026-11-12 · resume · Day 2 (two days ahead of the calendar; Thursday, so 1h budget)
- Evidence: Day 1 complete and pushed. Uncommitted: the .http now uses `{{NotificationSenderApi_HostAddress}}` (nit fixed) and this log. Missing for Day 2: `.github/`, ADRs (`docs/adr/` has only `.gitkeep`), CI badge
- Secret scan before going public: history clean (connection strings have empty `Password=`); commit author emails include a work address (user's call)
- Next: commit the pending changes, create the Supabase project (Singapore), then write `.github/workflows/ci.yml` and push until green
- Build/tests: build OK with 0 warnings; 9/9 unit tests pass

## 2026-11-13 · resume · Day 2 (calendar Day 1, Friday, 1h budget, one day ahead)
- Evidence: Supabase project created; `ci.yml` green on main; ruleset requires `build-and-test`; `dependabot.yml` with nuget/actions/compose, `exclude-patterns` groups, and an ignore rule for .NET-aligned majors (ADR-001); all Dependabot PRs merged. Merged main verified: build OK, 9/9 tests with xunit.runner 4
- Decision: Claude writes documentation markdown on request (ADRs, runbooks); the user keeps typing all code and config, and writes their own learning notes
- Done today: ADR-0001 to ADR-0009 and `docs/adr/README.md` drafted by Claude on branch `docs/ard-001-009` (typo in branch name)
- Left for Day 2: the user reads the ADRs, then the PR; CI badge in the README; make the repo public; Post #1
- Build/tests: build OK with 0 warnings; 9/9 unit tests pass
