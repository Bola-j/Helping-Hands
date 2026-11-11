# Helping Hands

Helping Hands is a proposed platform that helps people request assistance and lets platform coordinators refer requests to approved charities or individual donors and contributors. It records who handles each referral, its progress, and evidence of reported delivery and receipt.

The platform does not collect or hold money or goods, operate a warehouse, manage inventory or charity accounting, or take a share of contributions. The charity or contributor delivers assistance outside the platform.

## How it works

1. A charity self-registers. An authorized coordinator approves, rejects, or leaves the application pending; approval activates the charity account. The existing API has not yet been aligned with this Phase 1 target.
2. For Phase 1, a beneficiary submits their own request. Third-party and coordinator-on-behalf intake remain later scope. Requests can be for goods, money, or an organizational referral subtype such as tutoring or transport.
3. A coordinator reviews the request and may split mixed needs into linked requests. Any request type can be referred to an approved charity or individual contributor, and a request can be divided among several recipients.
4. Each recipient handles its assigned portion and supplies evidence that the assistance was delivered and received, including for money. The platform tracks the reported outcome and any unresolved need.

Individual contributors register and need coordinator approval before referral. Recipients provide an outcome description and attachment(s) addressing delivery and receipt; a coordinator reviews them before marking a referral complete. Access is limited by role and involvement. Evidence and review are not a guarantee of independently verified delivery.

## First version

The full first-version scope covers charity and contributor approvals, request submission and review, splitting and referrals, evidence, progress tracking, and role-appropriate access using fictional people and sample data. The Phase 1 target includes beneficiary self-intake, mixed needs, split referrals, charity capacity, reminders, evidence review and explicit closure. Contributor approval, third-party intake and release/reassignment remain later scope. Real donations, payment processing, subscription billing, detailed service delivery management, and live operations are outside the demonstration.

The potential service model may be supported by subscriptions, grants, or sponsorships; adoption and funding require validation. A real launch also requires partner input and appropriate local review.

## Project team

- **Bola Ghaly** — backend development.
- **Ziad Walid** — database and architecture design.
- **hussien okely** and **ali fathy** — web application UI.

These are areas of responsibility; the team collaborates on planning, integration, reviews, and testing. Bola Ghaly serves as Scrum Master.

## Project documents

- [Business Requirements Document](docs/BRD.md)
- [Product Requirements Document](docs/PRD.md)
- [Software Requirements Specification](docs/SRS.md)
- [System models](docs/models.md), [confirmed database structure and PostgreSQL mapping](docs/database-design.md), and [architecture and compatibility](docs/architecture.md)
- [API v1 plan](docs/api-v1-plan.md), [API demo Scrum roadmap](docs/api-demo-scrum-roadmap.md), [API demo walkthrough](docs/api-demo-walkthrough.md), and [full-app Scrum roadmap](docs/scrum-roadmap.md)
- [Backend delivery breakdown](docs/phase1-backend-delivery-breakdown.md) and [first foundation milestone](docs/phase1-foundation-milestone.md).
- [Full idea and business model — original reference](docs/references/helping-hands-full-idea.md). The current BRD supersedes its inventory, financial management, and direct service-coordination model.

## API demo (work in progress)

The .NET 10 API implements an initial charity-to-referral workflow and uses PostgreSQL plus S3-compatible evidence storage. It is **not yet a verified complete demo**: evidence storage, recovery, concurrency and end-to-end checks remain incomplete. Use fictional records only; do not use this build for real operations. See the [API demo roadmap](docs/api-demo-scrum-roadmap.md).

For local development, install the .NET 10 SDK, a compatible `dotnet-ef` tool and Docker with its engine running. Copy `.env.example` to `.env` and choose local credentials. PostgreSQL and the S3-compatible service bind to loopback ports 55432 and 9000; make sure they are available. Add distinct development-only `DemoSeed__...` values and `DEV_CERT_PASSWORD` in `.env`. Passwords must satisfy ASP.NET Identity password policy.

Create a local HTTPS certificate using the same password as `DEV_CERT_PASSWORD`. Do not commit this file or password:

```powershell
if (-not (Test-Path .env)) { Copy-Item .env.example .env }
# Edit .env and set the local service and fictional account credentials.
New-Item -ItemType Directory -Force .local/https | Out-Null
# Substitute the value you chose for DEV_CERT_PASSWORD; avoid storing it in shell history.
dotnet dev-certs https -ep .local/https/dev.pfx -p '<local-certificate-password>'
dotnet dev-certs https --trust
docker compose up -d postgres minio minio-init
./scripts/run-api.ps1 -Migrate
docker compose run --rm api --seed-demo
docker compose up -d api
```

For an isolated second stack, use a distinct Compose project name and set unique loopback `API_HOST_PORT`, `POSTGRES_HOST_PORT`, `MINIO_API_HOST_PORT` and `MINIO_CONSOLE_HOST_PORT` values in its separate environment file. Update the host-side `ConnectionStrings__HelpingHands` and `EvidenceStorage__ServiceUrl` to match those host ports; containers continue to use internal service names and ports. A project name isolates Compose-managed volumes but does not by itself prevent host-port collisions. Keep existing volumes intact.

The migration command runs on the host using the loopback connection from `.env`. The API container uses Compose service names for PostgreSQL and the S3 service; it does not run migrations automatically. The explicit seed command works only in Development and does not promote an existing unrelated account. The launcher reads unquoted `KEY=VALUE` entries from `.env` into the process, restores previous values on exit, and supports values containing `=`. `.env` and `.local` are ignored by Git and excluded from the Docker build context. Never commit credentials; use a secret manager or deployment environment for real deployments.

Use `https://localhost:8443/health` for liveness, `/ready` for PostgreSQL readiness, and `/openapi/v1.json` for the development OpenAPI document. Authentication uses secure cookies: an HTTP client must retain cookies, request `/api/v1/auth/antiforgery`, and send its token in the `X-CSRF-TOKEN` header on each unsafe request. If a requester creates a charity application, sign out and in again to refresh the representative role in the cookie. The future web UI should call the API through a same-origin HTTPS proxy; there is no frontend project or frontend container in the repository yet. To run the API directly on the host instead, use `./scripts/run-api.ps1` and `https://localhost:5087`.

Build with `dotnet build HelpingHands.slnx`. Stop the API with Ctrl+C and local services with `docker compose stop`; stopping retains volumes. Do not delete volumes containing data you need. The current file-processing policy is intended only for fictional demo evidence and is not approved for production.

## Repository status

The BRD is the business baseline. The PRD and SRS are finalized working requirements baselines, subject to normal change control; the database structure is confirmed; supporting models and the PostgreSQL mapping still require implementation and verification. The demo API is under development and has not yet passed complete end-to-end verification.

The Git commit dates in the initial project sequence were reconstructed at the project owner's request; they do not establish when work or validation actually occurred. Recorded stakeholder and owner validation dates in the documents retain their actual meaning.
