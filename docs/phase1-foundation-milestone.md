# First milestone: local backend foundation and API CI

[GitHub milestone 1](https://github.com/Bola-j/Helping-Hands/milestone/1) groups P1-02, P1-05, P1-06 and API CI.

**Goal:** establish a repeatable, isolated development foundation for the Phase 1 API: .NET build and tests, reviewed PostgreSQL migration, API/PostgreSQL/private S3-compatible storage through Docker Compose, and API CI. The target is the team's 12 November 2026 backend forecast, not a delivery commitment. This milestone prepares subsequent identity and workflow work; it does not claim a complete charity-to-closure demo or live deployment.

**Status:** in progress. The API baseline, local services, design and CI are open as separate draft pull requests; source changes also remain uncommitted in the original working tree. A 9 November assessment recorded nine passing tests and an isolated empty-database migration; repeat checks on the eventual reviewed candidate. During this planning pass Docker's Linux engine was unreachable, and the configured S3 image previously returned HTTP 401. No current end-to-end stack or CI result is claimed. See the [backend delivery breakdown](phase1-backend-delivery-breakdown.md) and [SRS assessment appendix](SRS.md#appendix-phase-1-implementation-baseline-and-verification).

## Execution evidence — 11 November 2026

- [API baseline PR #70](https://github.com/Bola-j/Helping-Hands/pull/70) includes the reviewed working-tree foundation. Local .NET 10 build and nine tests passed, and EF reports no pending model changes. An isolated PostgreSQL 17 migration and schema assertions passed. These are not an upgrade or recovery test.
- [API CI PR #71](https://github.com/Bola-j/Helping-Hands/pull/71) passed [GitHub Actions run 43036336578](https://github.com/Bola-j/Helping-Hands/actions/runs/43036336578) for commit `6b152416769f0d0174532e645a86a721d48ed83d`, including isolated PostgreSQL migration/schema checks. It remains a draft pending review and merge.
- [Design PR #72](https://github.com/Bola-j/Helping-Hands/pull/72) contains editable diagrams; XML/edge checks passed, but rendered visual review remains outstanding.
- [Local services PR #73](https://github.com/Bola-j/Helping-Hands/pull/73) makes host ports configurable and bounds storage initialization. Docker is running; the configured S3 images returned registry authorization errors and alternative pulls did not complete. A uniquely named Compose PostgreSQL instance preserved a fictional marker across restart; only the isolated test resources were removed. Full stack startup, container certificate readability, bucket connectivity, storage persistence and backup/restore remain unverified.
- An independent host-process check observed `/health` returning 200 and `/ready` returning 503 with PostgreSQL deliberately unreachable. This is not a containerized end-to-end check.

## Migration review and recovery approach

The generated initial migration SQL was inspected for the current candidate: it declares 16 tables including EF migration history, five business-reference sequences, 13 unique indexes, 25 foreign keys and 17 check constraints. The isolated PostgreSQL assertion script verified UUID primary keys on application tables and the expected sequences and unique reference indexes; those checks do not prove all application authorization or concurrency behavior.

For a later upgrade, take a timestamped PostgreSQL custom-format backup and a coordinated backup of private evidence, verify their provenance, review the new migration SQL, and restore both into isolated test services before applying the upgrade there. Compare migration history and representative records and exercise the relevant workflow. Apply an approved change to a shared environment only under its own authorization and documented recovery procedure. Only the initial migration exists now, so a preceding-to-newer migration test is **not yet applicable**; no restore or recovery exercise has been performed in this milestone.

## Deliverables and single-responsibility tickets

Owners are proposed by specialty, not GitHub assignments. Preserve the dirty working tree and use reviewed feature branches; no local cleanup, reset or shared database changes are part of this milestone.

| Priority | Ticket and proposed responsible | Task and dependencies | Observable acceptance |
| --- | --- | --- | --- |
| 0 | [P1-01: assessment](https://github.com/Bola-j/Helping-Hands/issues/1) — Bola | Existing prerequisite; use its findings and preserve assessed files. | Issue is closed; distinguish historical results from checks on the new candidate. |
| 1 | [P1-02: requirements and design](https://github.com/Bola-j/Helping-Hands/issues/2) — Bola | P1-01 is complete; reconcile the outstanding Phase 1 diagrams and hand off the data-model decisions needed for migration work. | Document remaining model assumptions and diagram changes; record the Scrum Master review and any unresolved design work without claiming implementation. |
| 1 | [P1-05: PostgreSQL baseline](https://github.com/Bola-j/Helping-Hands/issues/5) — Ziad | Depends on P1-01 and the open [P1-02 requirements/design issue](https://github.com/Bola-j/Helping-Hands/issues/2). Review EF migration and generated SQL, UUID keys, references and constraints. | Apply the migration to empty isolated PostgreSQL; record reviewed SQL and model/snapshot check. Document backup/recovery. Test a real upgrade when both a preceding and a newer migration exist; with only the initial migration, explicitly record upgrade testing as not yet applicable. |
| 1 | [P1-06: local API and dependencies](https://github.com/Bola-j/Helping-Hands/issues/6) — Ziad | Depends on P1-01. Resolve Docker engine and accessible pinned S3-compatible image. Validate API, PostgreSQL, private bucket, HTTPS and service-to-service settings. | An isolated stack starts with environment-supplied credentials, persistent volumes, distinct liveness/readiness and a private evidence bucket with basic storage connectivity; existing volumes are untouched. Full authorized evidence upload/download is P1-39/P1-40. |
| 1 | [P2-DEP-02: API CI](https://github.com/Bola-j/Helping-Hands/issues/50) — Bola | Pulled forward at the user's request. Prepare the build/test workflow independently; integrate database-backed checks after P1-05. Deployment is separate. | A real GitHub Actions run on a feature branch or PR restores, builds and runs focused API tests; migration checks use an isolated real PostgreSQL service. Retain the run link and results; mark S3 integration separately if unavailable. |
| 2 | [P1-46: migration and persistence recovery](https://github.com/Bola-j/Helping-Hands/issues/46) — Ziad | Full acceptance also depends on later database/storage work. Plan a foundation smoke check now; do not close the full ticket early. | Document isolated startup and persistence across restart; perform full recovery and upgrade acceptance after other dependencies. |
| 2 | [P1-56: service startup and restart](https://github.com/Bola-j/Helping-Hands/issues/59) — Ziad | Full acceptance additionally depends on mail, evidence and migration/recovery work; frontend proxy verification remains in P1-57. | Record isolated API/PostgreSQL/S3 readiness and restart checks now; complete the full ticket only when all prerequisites are available. |

The local email catcher [P1-25](https://github.com/Bola-j/Helping-Hands/issues/25) depends on reminder-state work in P1-24 and is sequenced in the backend breakdown. A frontend service is not included until a frontend project exists. Identity, catalog, capacity, needs, matching and outcomes follow under their own tickets.

## Sequence and handoffs

1. **Baseline and unblock:** Bola records the exact source candidate and build/test results. Ziad checks Docker and S3 image availability, with credentials supplied locally. Confirm that P1-02's prerequisite remains visible to the migration owner.
2. **Parallel work:** Ziad owns migration review and local services; Bola owns API CI. Agree on connection, HTTPS and storage configuration before integrating. Isolate PostgreSQL and storage tests from existing volumes and shared environments.
3. **Integrate:** Apply the reviewed migration against new isolated PostgreSQL, start the isolated stack, verify API liveness and readiness, then exercise private bucket connectivity and dependency failure; full API-mediated evidence scenarios belong to P1-39/P1-40. Run CI with temporary services.
4. **Review:** Hand off actual commands, candidate commit, CI run, reviewed SQL, test results, service failures and risks. The Scrum Master reviews the evidence; close each issue only when its complete acceptance criteria are met.

## Verification checklist

These are future checks, not recorded results. Run from the repository root. Do not expose `.env` values or use real participants' details.

```powershell
git status -sb
dotnet --version
docker version
dotnet build HelpingHands.slnx --nologo
dotnet test HelpingHands.slnx --no-build --nologo
dotnet ef migrations has-pending-model-changes --project src/HelpingHands.Api --no-build
docker compose config --quiet
```

The Compose configuration check requires the documented local `.env` variables; success does not start or validate services. Run startup or migration against a **separately named Compose project with an isolated environment file, unique host ports and new named volumes**; do not reuse the default project's data. Prepare a development-only HTTPS certificate and fictional credentials in ignored local storage as described in the [README](../README.md), without echoing or committing them. Verify the pinned storage image can be obtained and the bucket is private. Review generated migration SQL before applying it.

- [ ] Re-run build, tests and EF model/snapshot comparison; capture results and candidate commit.
- [ ] Apply migration to empty isolated PostgreSQL and inspect migration history and constraints; record the upgrade limitation or actual upgrade result.
- [ ] Confirm HTTPS API startup, `/health` liveness and `/ready` PostgreSQL readiness, including expected failure when PostgreSQL is unavailable.
- [ ] Confirm the evidence bucket is private and basic storage connectivity works. Test authorized API upload/download, denied access and cleanup later under P1-39/P1-40; do not mark them passed here.
- [ ] Restart only isolated containers and check PostgreSQL and evidence persistence; keep recovery/restore unverified until actually exercised.
- [ ] Run API CI on the reviewed branch/PR and retain its URL, output, failures and skipped service-dependent checks.
- [ ] Review evidence and limitations; do not mark later features, full recovery, web, production deployment or real-world readiness complete.

## Exit conditions and blockers

The milestone is complete when P1-05, P1-06 and P2-DEP-02 meet their full agreed scope, relevant checks above have actual evidence, and the Scrum Master accepts it. If P1-02, Docker, the S3 image, an applicable migration upgrade test or a real CI run is unavailable, retain the affected issue as blocked/in progress, record the reason and owner on the Project, and report the milestone incomplete. If the candidate has only the initial migration, record the upgrade test as not yet applicable and document the approach; do not report it as passed. Actual upgrade and recovery exercises remain in P1-46. The 12 November target is a forecast; no failed or unrun check is converted into a pass to meet it.
